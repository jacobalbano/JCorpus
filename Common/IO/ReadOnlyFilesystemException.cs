using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.IO;

public class ReadOnlyFilesystemException : Exception
{
    public ReadOnlyFilesystemException() : base("Cannot perform operation on read-only file or filesystem")
    {
    }
}
