using Common;
using Common.Addins.Accelerate;
using Common.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dawg;

public class Accelerator : IAccelerator
{
    public ICorpusContent ContentObject => throw new NotImplementedException();

    public void Accelerate(CorpusWork work, IEnumerable<CorpusEntry> content)
    {
        throw new NotImplementedException();
    }
}
