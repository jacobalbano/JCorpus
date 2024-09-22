using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Content;

/// <summary>
/// Furigana that corresponds to some portion of a <see cref="CorpusEntry"/>. Stored in a <see cref="FuriganaCollection"/> alongside a <see cref="CorpusWork"/>.
/// </summary>
/// <param name="Content">The furigana content</param>
/// <param name="EntryContentStartIndex">The index in the <see cref="CorpusEntry.Content"/> string where this furigana should begin</param>
/// <param name="EntryContentEndIndex">The index in the <see cref="CorpusEntry.Content"/> string where this furigana should end</param>
public record class CorpusFurigana(
    int EntryContentStartIndex,
    int EntryContentEndIndex,
    string Content
)
{
    /// <summary>
    /// Parses a string representation of a furigana object
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static CorpusFurigana Deserialize(string str)
    {
        int sliceStart = 0;
        var parts = new List<string>();
        for (int i = 0; i < 2; i++)
        {
            var index = str.IndexOf('|', sliceStart);
            if (index <= 0) throw new Exception("Failed to find delimiter");
            parts.Add(str[sliceStart..index]);
            sliceStart = index + 1;
        }

        parts.Add(str[sliceStart..]);

        if (!int.TryParse(parts[0], out var start) || !int.TryParse(parts[1], out var end))
            throw new Exception("Invalid number format");

        return new CorpusFurigana(start, end, parts[3]);
    }

    public static string Serialize(CorpusFurigana entry)
    {
        return string.Join('|', entry.EntryContentStartIndex, entry.EntryContentEndIndex, entry.Content);
    }
}
