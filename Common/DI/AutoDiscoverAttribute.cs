using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common.DI;

// TODO: documentation

[Flags]
public enum AutoDiscoverOptions
{
    None = 0,
    Singleton = 1 << 0,
    Scoped = 1 << 2,
    Transient = 1 << 3,
    Implementations = 1 << 4,
    ForceInitialize = 1 << 5,
}

[AttributeUsage(AttributeTargets.Assembly)]
public class AutoDiscoverAssemblyAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
public class AutoDiscoverAttribute : Attribute
{
    public AutoDiscoverAttribute(AutoDiscoverOptions options)
    {
        Options = options;
    }

    public Type ImplementationFor { get; set; }
    public AutoDiscoverOptions Options { get; }
}