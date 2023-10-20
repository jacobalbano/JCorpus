using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Content;

namespace Common;

public interface IFuriganaCollectionProvider
{
    IEnumerable<CorpusFurigana> GetFuriganaCollection();
}
