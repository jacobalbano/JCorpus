using Common.IO;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MokuroWrapper;

public record class MokuroJsonFile(
    FilePath FileName,
    MokuroPageData Json
);

public record class MokuroPageData(
    [property: JsonPropertyName("version")]
    string Version,

    [property: JsonPropertyName("img_width")]
    int ImageWidth,

    [property: JsonPropertyName("img_height")]
    int ImageHeight,

    [property: JsonPropertyName("blocks")]
    IReadOnlyList<MokuroBlock> Blocks
);
