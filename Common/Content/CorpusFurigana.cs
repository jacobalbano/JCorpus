using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Content;

/// <summary>
/// Furigana that corresponds to some portion of a <see cref="CorpusEntry"/>. Stored separately.
/// </summary>
/// <param name="Content">The furigana content</param>
/// <param name="EntryId">An id that corresponds to the <see cref="CorpusEntry.ScopedUniqueId"/> field of the <see cref="CorpusEntry"/> that this object belongs to</param>
/// <param name="EntryContentStartIndex">The index in the <see cref="CorpusEntry.Content"/> string where this furigana should begin</param>
/// <param name="EntryContentEndIndex">The index in the <see cref="CorpusEntry.Content"/> string where this furigana should end</param>
public record class CorpusFurigana(
    string Content,
    UniqueId EntryId,
    int EntryContentStartIndex,
    int EntryContentEndIndex
);