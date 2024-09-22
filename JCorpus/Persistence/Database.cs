﻿using Common.DI;
using Common.IO;
using JCorpus.Persistence.Models;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Utility.IO;

namespace JCorpus.Persistence;

[AutoDiscover(AutoDiscoverOptions.Singleton)]
internal class Database
{
    public Database(IPersistentDirectory<Database> directory)
    {
        db = new LiteDatabase(directory.File("Data.db").GetFullyQualifiedPath());
        db.Mapper.ResolveCollectionName = CollectionNameResolver;
        db.Checkpoint();
    }

    private static string CollectionNameResolver(Type t)
    {
        var result = t.Name;
        for (var decl = t.DeclaringType; decl != null; decl = decl.DeclaringType)
            result = $"{decl.Name}_{result}";

        return result;
    }

    public ILiteQueryable<T> Select<T>()
    {
        return Establish<T>().Query();
    }

    public SingletonWrapper<T> GetSingleton<T>() where T : ModelBase, new()
    {
        return new SingletonWrapper<T>(this);
    }

    public ISession BeginSession() => new SessionImpl(this);

    private ILiteCollection<T> Establish<T>()
    {
        var collection = db.GetCollection<T>();
        Optimizer<T>.Run(db.Mapper, collection);
        return collection;
    }

    public interface ISession : IDisposable
    {
        public T Insert<T>(T item);
        public T InsertOrUpdate<T>(T item);
        public bool Delete<T>(T item) where T : ModelBase;
        public int DeleteAll<T>() where T : ModelBase;
        public bool Update<T>(T item) where T : ModelBase;
        public ILiteQueryable<T> Select<T>();
        public SingletonWrapper<T> GetSingleton<T>() where T : ModelBase, new();
    }

    private class SessionImpl : ISession
    {
        public ILiteQueryable<T> Select<T>() => owner.Select<T>();
        public SingletonWrapper<T> GetSingleton<T>() where T : ModelBase, new() => owner.GetSingleton<T>();

        public SessionImpl(Database db)
        {
            owner = db;
        }

        public T Insert<T>(T item)
        {
            owner.Establish<T>().Insert(item);
            return item;
        }

        public T InsertOrUpdate<T>(T item)
        {
            owner.Establish<T>().Upsert(item);
            return item;
        }

        public bool Delete<T>(T item) where T : ModelBase
        {
            return owner.Establish<T>().Delete(item.Key);
        }

        public int DeleteAll<T>() where T : ModelBase
        {
            return owner.Establish<T>().DeleteAll();
        }

        public bool Update<T>(T item) where T : ModelBase
        {
            return owner.Establish<T>().Update(item);
        }

        public void Dispose()
        {
            //if (Program.BotConfig.CheckpointEveryMutation)
                //owner.db.Checkpoint();
        }

        private readonly Database owner;
    }

    public class SingletonWrapper<T> : IDisposable where T : ModelBase, new()
    {
        public T Value { get; }

        public SingletonWrapper(Database database)
        {
            owner = database;
            Value = database.Select<T>().SingleOrDefault() ?? new T();
        }

        public void Dispose()
        {
            using var s = owner.BeginSession();
            s.InsertOrUpdate(Value);
        }

        private readonly Database owner;
    }

    private class Optimizer<T>
    {
        public static void Run(BsonMapper mapper, ILiteCollection<T> collection)
        {
            if (hasRun) return;

            foreach (var propInfo in typeof(T).GetProperties())
            {
                if (propInfo.GetCustomAttribute<ModelBase.IndexedAttribute>() != null)
                {
                    var param = Expression.Parameter(typeof(T));
                    var convert = Expression.TypeAs(Expression.Property(param, propInfo), typeof(object));
                    var getMethod = Expression.Lambda<Func<T, object>>(convert, param);
                    collection.EnsureIndex(getMethod);
                }

                if (propInfo.GetCustomAttribute<BsonConverterAttribute>() is BsonConverterAttribute attr)
                {
                    if (!converters.TryGetValue(attr.ConverterType, out var converter))
                    {
                        converter = (IBsonConverter)Activator.CreateInstance(attr.ConverterType)!;
                        converters[attr.ConverterType] = converter;
                    }

                    var type = propInfo.PropertyType;
                    if (Nullable.GetUnderlyingType(type) is Type underType)
                        type = underType;

                    mapper.RegisterType(type, converter.Serialize, converter.Deserialize);
                }
            }
            hasRun = true;
        }

        private static bool hasRun = false;
        private static readonly Dictionary<Type, IBsonConverter> converters = new();
    }

    private readonly LiteDatabase db;
}