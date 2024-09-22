using Common.Addins.Extract;
using Common.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Narou;

public class Extractor : ICorpusWorkExtractor
{
    public IReadOnlyList<string> InTags => Constants.Tags;

    public ICorpusWorkExtractor.IContext Extract(Stream stream, IExtractProgress progress)
    {
        throw new NotImplementedException();
    }
}
