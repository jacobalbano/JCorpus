using Common.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility.IO;

/// <summary>
/// Creates a writable, persistent directory for addins to store data in.
/// <typeparamref name="T"/> represents the name of the directory, e.g.
/// <code>.environment/(PluginName)/<typeparamref name="T"/></code>
/// </summary>
public interface IPersistentDirectory<T> : IVirtualFs
{
}
