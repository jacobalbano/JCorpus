using Common;
using Common.Content;
using Common.Addins.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mokuro;

public class ContextProvider : IContextProvider
{
    public IEnumerable<CorpusEntryContext> GetEvidence(CorpusWorkId corpusWorkId, CorpusEntryId corpusEntryId)
    {
        if (!corpus.TryGetWorkExtraContent(corpusWorkId, out EvidenceCollection collection))
            yield break;

        using (collection)
        {
            if (!collection.TryGetImage(corpusEntryId, out var image))
                yield break;

            yield return new(
                ContextKind.Image,
                image.EncodedData.ToArray()
            );
        }
    }

    public ContextProvider(ICorpus corpus)
    {
        this.corpus = corpus;
    }

    private readonly ICorpus corpus;
}
