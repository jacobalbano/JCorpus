using Common;
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

public class CorpusEntryCollection : IReadOnlyCollection<CorpusEntry>, ICorpusContent
{
    public string ContentFileName => "@extract.txt";

    /// <summary>
    /// Add an item to the collection.
    /// </summary>
    /// <exception cref="ObjectFrozenException"></exception>
    public void Add(CorpusEntry item)
    {
        IFreezable.ThrowIfFrozen(this);
        if (ids.Add(item.ScopedUniqueId))
            entries.Add(item);
    }

    private readonly List<CorpusEntry> entries = new();
    private readonly HashSet<CorpusEntryId> ids = new();

    #region IReadOnlyCollection
    public int Count => entries.Count;

    public bool Contains(CorpusEntry item) => entries.Contains(item);
    public void CopyTo(CorpusEntry[] array, int arrayIndex) => entries.CopyTo(array, arrayIndex);
    public IEnumerator<CorpusEntry> GetEnumerator() => entries.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)entries).GetEnumerator();
    #endregion

    #region ICorpusContent
    void ICorpusContent.Read(IVirtualFile file)
    {
        IFreezable.ThrowIfFrozen(this);
        foreach (var str in file.ReadAllLines())
            Add(CorpusEntry.Deserialize(str));
    }

    void ICorpusContent.Write(IVirtualFile file)
    {
        file.WriteAllLines(this.Select(CorpusEntry.Serialize));
    }
    #endregion

    #region IFreezable
    void IFreezable.Freeze() => frozen = true;
    bool IFreezable.Frozen => frozen;
    private bool frozen;
    #endregion
}
