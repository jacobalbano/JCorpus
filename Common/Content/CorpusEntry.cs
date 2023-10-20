using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Content;

/// <summary>
/// Some portion of text that will be stored in the corpus.
/// </summary>
/// <param name="ScopedUniqueId">An arbitrary string that MUST uniquely represent this CorpusEntry within its owning <see cref="CorpusWork"/></param>
/// <param name="Content">The content of this entry. Should not contain furigana, formatting characters, etc.</param>
public record class CorpusEntry(
    UniqueId ScopedUniqueId,
    string Content
)
{
    public static CorpusEntry Deserialize(string str)
    {
        var index = str.IndexOf('|');
        if (index <= 0)
            throw new Exception("Failed to find Id termination character");

        // TODO: escape newlines?
        return new CorpusEntry(
            str[..index],
            str[(index + 1)..]
        );
    }

    public static string Serialize(CorpusEntry entry)
    {
        // TODO: unescape newlines?
        return $"{entry.ScopedUniqueId}|{entry.Content}";
    }
}
