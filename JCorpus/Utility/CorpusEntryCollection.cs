using Common.Content;
using Common.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace JCorpus.Utility;

internal class CorpusEntryCollection : ICollection<CorpusEntry>, IDisposable
{
    public CorpusEntryCollection(IVirtualFile file)
    {
        if (file.Exists)
        {
            foreach (var str in file.ReadAllLines())
                Add(CorpusEntry.Deserialize(str));
        }

        this.file = file;
    }

    public void Dispose()
    {
        file.WriteAllLines(entries.Select(x => CorpusEntry.Serialize(x)));
    }

    public void Add(CorpusEntry item)
    {
        if (ids.Add(item.ScopedUniqueId))
            entries.Add(item);
    }

    private readonly IVirtualFile file;
    private readonly ICollection<CorpusEntry> entries = new List<CorpusEntry>();
    private readonly HashSet<UniqueId> ids = new();

    #region ICollection
    public int Count => entries.Count;
    public void Clear() => entries.Clear();
    public bool Contains(CorpusEntry item) => entries.Contains(item);
    public void CopyTo(CorpusEntry[] array, int arrayIndex) => entries.CopyTo(array, arrayIndex);
    public bool Remove(CorpusEntry item) => entries.Remove(item);
    public IEnumerator<CorpusEntry> GetEnumerator() => entries.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)entries).GetEnumerator();
    public bool IsReadOnly => entries.IsReadOnly;
    #endregion
}
