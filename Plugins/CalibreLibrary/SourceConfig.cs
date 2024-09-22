using CalibreLibrary.Metadata;
using Common.Configuration;
using Common.DI;
using Common.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CalibreLibrary;

public record class SourceConfig(
    string LibraryRoot,
    Identifier[] IdFilters,
    Extension[] ExtensionFilters,
    OrderBy OrderBy,
    string IdentifierScheme
);

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Extension
{
    Epub,
    Cbz
}

[SchemaDescribe]
public record class OrderBy(
    OrderByType Type,
    OrderByDirection Direction = OrderByDirection.Descending
);

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrderByType
{
    None,
    Rating,
    Date,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrderByDirection
{
    Ascending,
    Descending
}