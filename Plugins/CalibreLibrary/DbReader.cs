using CalibreLibrary.Metadata;
using Common.DI;
using Common.IO;
using Microsoft.Data.Sqlite;
using NodaTime.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility.IO;

namespace CalibreLibrary.SQL;

[AutoDiscover(AutoDiscoverOptions.Singleton)]
public class DbReader
{
    public DbReader()
    {
        sqlQuery = new EmbeddedResourceFilesystem(GetType().Assembly)
            .Directory("SQL")
            .File("getbooks.sql")
            .ReadAllText();
    }

    public Dictionary<FilePath, BookMetadata> GetBookMetadata(IVirtualFs libraryDir)
    {
        var dbFile = libraryDir.File("metadata.db").GetFullyQualifiedPath();
        var result = new Dictionary<FilePath, BookMetadata>();
        using (var connection = new SqliteConnection($"Mode=ReadOnly; Data Source={dbFile}"))
        {
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = sqlQuery;

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var row = new Row(
                    reader.GetInt32(0),
                    reader.GetDateTime(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetString(4),
                    reader.GetString(5),
                    reader.IsDBNull(6) ? null : (byte)reader.GetInt16(6),
                    reader.GetString(7),
                    reader.GetString(8)
                );

                result.Add(
                    $"{row.Path}/{row.Filename}.{row.Format.ToLower()}",
                    new BookMetadata(
                        row.Rating,
                        row.Timestamp.ToUniversalTime().ToInstant(),
                        row.Title,
                        row.Authors,
                        row.Ids.Split(",")
                            .Select(Identifier.Parse)
                            .ToList()
                    )
                );
            }
        }

        return result;
    }

    record class Row(
        int BookId,
        DateTime Timestamp,
        string Title,
        string Path,
        string Filename,
        string Format,
        byte? Rating,
        string Authors,
        string Ids
    );

    readonly string sqlQuery;
}
