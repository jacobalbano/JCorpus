using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Utility;

public interface IExtractProgress
{
    void ReportProgress(string identifier);

    IEnumerable<string> GetIds();
}
