using Common;
using Common.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Content;
using System.Diagnostics.CodeAnalysis;
using System.Collections;
using System.Text.RegularExpressions;

namespace Frequency;

/// <summary>
/// Contains a count of every character contained in a <see cref="CorpusWork"/>.
/// </summary> 
public class CharacterFrequency : ICorpusContent, IReadOnlyDictionary<string, ulong>
{
    public string ContentFileName => "CharacterFrequency.txt";

    internal void ReadCorpusContent(IEnumerable<CorpusEntry> content)
    {
        foreach (var e in content)
        {
            var runes = e.Content.EnumerateRunes()
                .Select(x => x.ToString())
                .Where(x => pattern.IsMatch(x));

            foreach (var str in runes)
            {
                var current = counts.TryGetValue(str, out var value) ? value : 0;
                counts[str] = current + 1;
            }
        }
    }

    #region ICorpusContent
    void ICorpusContent.Read(IVirtualFile inFile)
    {
        foreach (var line in inFile.ReadAllLines())
        {
            var parts = line.Split('|');
            counts.Add(parts[0], ulong.Parse(parts[1]));
        }
    }

    void ICorpusContent.Write(IVirtualFile outFile)
    {
        outFile.WriteAllLines(this
            .OrderByDescending(x => x.Value)
            .Select(x => $"{x.Key}|{x.Value}"));
    }
    #endregion

    #region IFreezable
    bool IFreezable.Frozen => frozen;
    void IFreezable.Freeze() => frozen = true;
    private bool frozen;
    #endregion

    #region Dictionary
    private readonly Dictionary<string, ulong> counts = new();
    public IEnumerable<string> Keys => ((IReadOnlyDictionary<string, ulong>)counts).Keys;
    public IEnumerable<ulong> Values => ((IReadOnlyDictionary<string, ulong>)counts).Values;
    public int Count => ((IReadOnlyCollection<KeyValuePair<string, ulong>>)counts).Count;
    public ulong this[string key] => ((IReadOnlyDictionary<string, ulong>)counts)[key];
    public bool ContainsKey(string key) => ((IReadOnlyDictionary<string, ulong>)counts).ContainsKey(key);
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out ulong value) => ((IReadOnlyDictionary<string, ulong>)counts).TryGetValue(key, out value);
    public IEnumerator<KeyValuePair<string, ulong>> GetEnumerator() => ((IEnumerable<KeyValuePair<string, ulong>>)counts).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)counts).GetEnumerator();
    #endregion

    private static readonly Regex pattern = new(@"[一-龯ぁ-んァ-ンｧ-ﾝﾞﾟぁ-ゞｦ-ﾟ々〆〤ヴヵヶ〇ぁ-ゟァ-ヿ㐀-䶵一-鿋豈-頻⺀-⿕ㇰ-ㇿ㈠-㉃㊀-㍿]+", RegexOptions.Compiled);
}
