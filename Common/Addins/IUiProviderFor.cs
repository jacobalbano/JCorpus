using Common.DI;
using Common.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Addins;

/// <summary>
/// Describes a means by which a plugin can provide custom files to be used in the UI.<br />
/// The filesystem will be accessible at <code>/api/UI/<typeparamref name="T"/></code>
/// </summary>
public interface IUiProviderFor<T> : IUiProvider where T : IPlugin<T>
{
    string IUiProvider.DirectoryName => typeof(T).Name;
}

/// <summary>
/// This type exists for implementation purposes. Use <seealso cref="IUiProviderFor{T}"/> instead.
/// </summary>
[AutoDiscover(AutoDiscoverOptions.Implementations | AutoDiscoverOptions.Singleton)]
public interface IUiProvider
{
    string DirectoryName { get; }

    /// <summary>
    /// Return a virtual filesystem which contains files intended to be used by this plugin's UI.
    /// </summary>
    IVirtualFs Root { get; }
}