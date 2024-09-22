using Common.Configuration;
using Common.Content;
using Common.DI;
using Common;
using JCorpus.Jobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Common.Addins.Search;

namespace JCorpus.Implementation.Search;


internal class RegexSearch : ISearch, IConfigurableWith<RegexSearch.SearchConfig>
{
    void IConfigurableWith<SearchConfig>.Configure(SearchConfig config)
        => Pattern = new(config.Pattern, RegexOptions.Compiled);

    public Regex Pattern { get; private set; }

    public IEnumerable<SearchHit> YieldHitsInWork(ICorpus corpus, CorpusWork work)
    {
        foreach (var entry in corpus.GetWorkContent(work.UniqueId))
        {
            var match = Pattern.Match(entry.Content);
            if (match.Success)
            {
                var captures = (match.Groups.Count > 1
                    ? match.Groups.Cast<Capture>().Skip(1)
                    : match.Captures.Cast<Capture>())
                    .Select(x => new SearchMatchRange(x.Index, x.Length))
                    .ToList();

                yield return new SearchHit(work.UniqueId, entry.ScopedUniqueId, entry.Content, captures);
            }
        }
    }

    [SchemaDescribe, AutoDiscover(AutoDiscoverOptions.Transient)]
    public record class SearchConfig(string Pattern = "");
}
