using CalibreLibrary.Utility;
using Common.DI;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Utility;

namespace CalibreLibrary.Metadata;

public record class BookMetadata(
    byte? Rating,
    [property: JsonConverter(typeof(NodaInstantJsonConverter))]
    Instant Timestamp,
    string Title,
    string Authors,
    IReadOnlyList<Identifier> Identifiers
);
