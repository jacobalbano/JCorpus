using Common.Content;
using Common.DI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Addins.Search;
/// <summary>
/// Defines a method by which a <see cref="CorpusWork"/> can be searched.<br />
/// </summary>
[AutoDiscover(AutoDiscoverOptions.Implementations | AutoDiscoverOptions.Scoped)]
public interface ISearch : IAddin
{
    IEnumerable<SearchHit> YieldHitsInWork(ICorpus corpus, CorpusWork work);
}

/// <summary>
/// Indicates that some <see cref="CorpusEntry"/> produced a positive hit for a search method.
/// </summary>
/// <param name="CorpusWorkId">The id of the <see cref="CorpusWork"/> that this hit belongs to.</param>
/// <param name="LineReference">The id of the <see cref="CorpusEntry"/> that produced the hit.</param>
/// <param name="Line">The full content of the line that produced the hit.</param>
/// <param name="MatchRanges">The ranges of the text which caused the hit to be positive.</param>
public record class SearchHit(
    CorpusWorkId CorpusWorkId,
    CorpusEntryId LineReference,
    string Line,
    IReadOnlyList<SearchMatchRange> MatchRanges
);

/// <summary>
/// Indicates a range in a given string which returned a true hit for a given search method.
/// </summary>
/// <param name="Start">The beginning index of the hit in the full line.</param>
/// <param name="Length">The end index of the hit in the full line.</param>
public record class SearchMatchRange(
    int Start,
    int Length
);