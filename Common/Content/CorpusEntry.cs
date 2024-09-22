using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.Content;

[JsonConverter(typeof(Converter))]
public record class CorpusEntryId(string Value) : UniqueIdBase<CorpusEntryId>(Value)
{
    public static implicit operator CorpusEntryId(string value) => new(value);
    public static implicit operator string(CorpusEntryId id) => id.Value;

    private class Converter : ConverterBase
    {
        protected override CorpusEntryId Create(string value) => new(value);
    }
}

/// <summary>
/// Some portion of text that will be stored in the corpus.
/// </summary>
/// <param name="ScopedUniqueId">
/// An arbitrary string that MUST uniquely represent this CorpusEntry within its owning <see cref="CorpusWork"/><br/>
/// This ID will be the key with which all supplementary information is linked with each line in the work.
/// </param>
/// <param name="Content">
/// The content of this entry. Should not contain furigana, formatting characters, etc.
/// </param>
public sealed record class CorpusEntry(
    CorpusEntryId ScopedUniqueId,
    string Content
)
{
    public static CorpusEntry Deserialize(string str)
    {
        var index = str.IndexOf('|');
        if (index <= 0)
            throw new Exception("Failed to find Id termination character");

        return new CorpusEntry(
            str[..index],
            str[(index + 1)..]
        );
    }

    public static string Serialize(CorpusEntry entry)
    {
        return $"{entry.ScopedUniqueId}|{entry.Content}";
    }
}
