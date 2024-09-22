using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Common.Addins;
using Common.Addins.Extract;
using Common.IO;

namespace Common.Content;

public static class KnownTags
{
    /// <summary>Optical Character Recognition was involved in the extraction process.</summary>
    public const string OCR = nameof(OCR);
}

/// <summary>
/// Represents details about a <see cref="CorpusWork"/>
/// </summary>
/// <param name="SourceAddinKey">The addin key for the <see cref="ICorpusWorkSource"/> that produced this work.</param>
/// <param name="ExtractorAddinKey">The addin key for the <see cref="ICorpusWorkExtractor"/> that produced this work.</param>
/// <param name="Tags">Optional arbitrary tags for this work. May include (but are not limited to) <see cref="KnownTags"/>.</param>
public record class CorpusWorkMetadata(
    AddinKey SourceAddinKey,
    AddinKey ExtractorAddinKey,
    IReadOnlyList<string> Tags
)
{
    public const string ContentFileName = "@meta.json";

    public static CorpusWorkMetadata Read(IVirtualFile inFile)
        => JsonSerializer.Deserialize<CorpusWorkMetadata>(inFile.ReadAllText());

    public static void Write(CorpusWorkMetadata metadata, IVirtualFile outFile)
        => outFile.WriteAllText(JsonSerializer.Serialize(metadata));
}