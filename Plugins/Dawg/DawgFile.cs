using Common;
using Common.Content;
using Common.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dawg;

internal class DawgFile : ICorpusContent
{
    public string ContentFileName => "Dawgfile.bin";

    #region ICorpusContent
    void ICorpusContent.Read(IVirtualFile file)
    {
        IFreezable.ThrowIfFrozen(this);
    }

    void ICorpusContent.Write(IVirtualFile file)
    {
    }
    #endregion

    #region IFreezable
    void IFreezable.Freeze() => frozen = true;
    bool IFreezable.Frozen => frozen;
    private bool frozen;
    #endregion
}
