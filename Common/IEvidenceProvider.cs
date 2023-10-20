using Common.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common;

public interface IEvidenceProvider
{
    // TODO: Address this
    [Obsolete("Should be made async")]
    public void WriteEvidenceToFolder(IVirtualFs outputDir);
}
