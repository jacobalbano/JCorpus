using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Utility;

// TODO: documentation

public interface IExtractProgress
{
    void ReportProgress(string identifier);

    IEnumerable<string> GetIds();
}
