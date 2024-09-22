using Common.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Content.Collections;

public class FuriganaCollection : ICorpusContent, IReadOnlyCollection<KeyValuePair<CorpusEntryId, IReadOnlyList<CorpusFurigana>>>
{
    public delegate string Formatter(string text, string furigana);

    public string ContentFileName => "Furigana.txt";

    public void Add(CorpusEntryId entryId, IReadOnlyList<CorpusFurigana> furigana)
    {
        IFreezable.ThrowIfFrozen(this);
        data[entryId] = furigana;
    }

    void ICorpusContent.Read(IVirtualFile inFile)
    {
        IFreezable.ThrowIfFrozen(this);
        var groups = inFile.ReadAllLines()
            .Select(ParseLine)
            .GroupBy(x => x.Item1)
            .Select(x => (x.Key, x.Select(y => y.Item2).ToList()));

        foreach (var (id, list) in groups)
            Add(id, list);

        static (CorpusEntryId, CorpusFurigana) ParseLine(string line, int lineNum)
        {
            var parts = line.Split("|");
            var id = new CorpusEntryId(parts.FirstOrDefault()
                ?? throw new Exception($"Line {line}: Failed to parse {nameof(CorpusEntryId)}"));

            var remainder = string.Join("", parts.Skip(1));
            try { return (id, CorpusFurigana.Deserialize(remainder)); }
            catch { throw new Exception($"Line {line}: Failed to deserialize {nameof(CorpusFurigana)}"); }
        }
    }

    public bool TryReconstructText(CorpusEntry entry, out string result, Formatter formatter)
    {
        throw new NotImplementedException();
    }

    void ICorpusContent.Write(IVirtualFile outFile)
    {
        var sb = new StringBuilder();
        foreach (var (id, list) in this.OrderBy(x => x.Key))
        {
            foreach (var item in list.Select(CorpusFurigana.Serialize))
                sb.AppendLine($"{id}|{item}");
        }

        outFile.WriteAllText(sb.ToString());
    }

    #region IReadOnlyCollection
    public int Count => data.Count;
    public IEnumerator<KeyValuePair<CorpusEntryId, IReadOnlyList<CorpusFurigana>>> GetEnumerator() => data.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => data.GetEnumerator();
    #endregion

    #region IFreezable
    void IFreezable.Freeze() => frozen = true;
    bool IFreezable.Frozen => frozen;
    private bool frozen;
    #endregion

    private readonly Dictionary<CorpusEntryId, IReadOnlyList<CorpusFurigana>> data = new();
}
