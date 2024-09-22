using Common.DI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Addins;

/// <summary>
/// Identifies a class library as a plugin for use by the corpus application.
/// </summary>
/// <typeparam name="TSelf">This plugin class</typeparam>
[AutoDiscover(AutoDiscoverOptions.Implementations | AutoDiscoverOptions.Transient)]
public interface IPlugin<TSelf> : IPlugin where TSelf : IPlugin<TSelf>
{
    string IPlugin.PluginName => typeof(TSelf).Name;
}

/// <summary>
/// Base interface, for use by the host application only. <br />
/// Implement <seealso cref="IPlugin{TSelf}"/> instead.
/// </summary>
public interface IPlugin
{
    /// <summary>The name of the plugin. Must be unique throughout the entire application. </summary>
    string PluginName { get; }

    /// <summary>The name of the plugin author.</summary>
    string Author { get; }

    /// <summary>A short description of what the plugin does.</summary>
    string Description { get; }
}