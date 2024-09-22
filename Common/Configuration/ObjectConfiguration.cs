using Common.Addins;
using Common.DI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Common.Configuration;


/// <summary>
/// Indicates that a type is intended to be created dynamically from an <see cref="ObjectConfiguration{T}"/>,<br />
/// and that an object of type <typeparamref name="TConfig"/> is required to complete the configuration.
/// <br/>
/// The configuration method cannot be run until after the object's constructor has finished!
/// </summary>
/// <typeparam name="TConfig">The type of the configuration object that should be deserialized and injected.</typeparam>
public interface IConfigurableWith<TConfig> : IConfigurable
{
    /// <summary>
    /// Configure this object.  
    /// Depending on the caller, this method may be called multiple times. Implementation should support updating the object's behavior based on this change.
    /// </summary>
    /// <param name="config"></param>
    void Configure(TConfig config);
    void IConfigurable.Configure(object config) => Configure((TConfig)config);
}

/// <summary>
/// Use <see cref="IConfigurableWith{TConfig}"/> instead!
/// </summary>
public interface IConfigurable
{
    void Configure(object config);
}

/// <summary>
/// Defines a recipe by which an object of type <typeparamref name="T"/> can be instantiated.
/// </summary>
/// <typeparam name="T">The interface or base class this configuration is for.</typeparam>
/// <param name="PluginName">The The name of the plugin that defines the specified addin.</param>
/// <param name="AddinName">The type name of the concrete type to create. Must be registered in the DI container.</param>
/// <param name="ConfigurationJson">An optional json object representing the configuration for the object, if it implements <see cref="IConfigurableWith{TConfig}"/>.</param>
public record class ObjectConfiguration<T>(
    string PluginName,
    string AddinName,
    JsonDocument ConfigurationJson = null
) where T : IAddin;