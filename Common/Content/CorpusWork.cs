using System.Text.Json.Serialization;

namespace Common.Content;


[JsonConverter(typeof(Converter))]
public record class CorpusWorkId(string Value) : UniqueIdBase<CorpusWorkId>(Value)
{
    public static implicit operator CorpusWorkId(string value) => new(value);
    public static implicit operator string(CorpusWorkId id) => id.Value;

    private class Converter : ConverterBase
    {
        protected override CorpusWorkId Create(string value) => new(value);
    }
}

/// <summary>
/// A collection of corpus text. May represent one volume of manga or novel, one chapter of a webnovel, etc.
/// </summary>
/// <param name="UniqueId">An arbitrary id that must uniquely identify this work in the corpus.</param>
public record class CorpusWork(
    CorpusWorkId UniqueId,
    CorpusWorkMetadata Metadata
);