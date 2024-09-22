using Common.Configuration.Schema;
using Common.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCorpus.Web.Schema;

record class ScalarTypeDefinition(string Type) : SchemaTypeDefinition(Type)
{
    public class BooleanHandler : IPropertyTypeHandler
    {
        public bool TryCreateTypeDefinition(Type type, out SchemaTypeDefinition def)
        {
            def = default;
            if (type != typeof(bool))
                return false;

            def = new ScalarTypeDefinition("bool");
            return true;
        }
    }

    public class StringHandler : IPropertyTypeHandler
    {
        public bool TryCreateTypeDefinition(Type type, out SchemaTypeDefinition def)
        {
            def = default;
            if (!StringTypes.Contains(type))
                return false;

            def = new ScalarTypeDefinition("string");
            return true;
        }

        private static readonly HashSet<Type> StringTypes = new()
        {
            typeof(string),  typeof(CorpusEntryId),  typeof(CorpusWorkId)
        };
    }

    public class NumericHandler : IPropertyTypeHandler
    {
        public bool TryCreateTypeDefinition(Type type, out SchemaTypeDefinition def)
        {
            def = default;
            if (!NumericTypes.Contains(type))
                return false;

            def = new ScalarTypeDefinition("number");
            return true;
        }

        private static readonly HashSet<Type> NumericTypes = new()
        {
            typeof(int),  typeof(double),  typeof(decimal),
            typeof(long), typeof(short),   typeof(sbyte),
            typeof(byte), typeof(ulong),   typeof(ushort),
            typeof(uint), typeof(float)
        };
    }
}
