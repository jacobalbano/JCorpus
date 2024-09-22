using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Addins;

/// <summary>
/// Uniquely identifies an <see cref="IAddin">Addin</see> in the application.
/// </summary>
/// <param name="PluginName">The name of the <see cref="IPlugin{TSelf}">Plugin</see> that the addin belongs to.</param>
/// <param name="AddinTypeName">The typename of the addin.</param>
public record class AddinKey(
    string PluginName,
    string AddinTypeName
)
{
    public override string ToString() => $"{PluginName}/{AddinTypeName}";
}

/// <summary>
/// Marker interface for interfaces which are treated as <strong>Addins</strong>.<br />
/// Addins form the core extensibility points of the application.<br />
/// Note: This interface is for internal use. Implementing it directly will have no effect.
/// </summary>
public interface IAddin
{
}
