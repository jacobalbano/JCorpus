using CalibreLibrary.Metadata;
using Common.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CalibreLibrary;

public record class EnumeratorConfig(
    IVirtualFs LibraryRoot,
    Identifier[]? IdFilters = null,
    string[]? ExtensionFilters = null,
    OrderBy? OrderBy = null
);

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