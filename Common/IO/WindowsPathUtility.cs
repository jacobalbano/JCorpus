using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.IO;

namespace Common.IO;

public static class WindowsPathUtility
{
    public static FilePath MakeFilePath(string windowsPath)
    {
        return windowsPath.Replace('\\', '/').Trim('/');
    }

    public static DirectoryPath MakeDirectoryPath(string windowsPath)
    {
        return windowsPath.Replace('\\', '/').Trim('/');
    }
}
