using Common.DI;
using Common.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility.IO;

/// <summary>
/// Describes a means of obtaining read-only access to the host filesystem.<br />
/// </summary>
/// <typeparam name="T">The calling type</typeparam>
public interface IFileSystemAccess<T>
{
    /// <summary>
    /// Create a read-only filesystem access object at the specified path.
    /// </summary>
    /// <param name="path">The path to access</param>
    public IVirtualFs Access(DirectoryPath path);
}
