using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility;

/// <summary>
/// Provides a mechanism for classes to receive configuration objects from the job parameters.
/// </summary>
/// <typeparam name="T">The config class</typeparam>
public interface IConfigProvider<T>
{
    T Get();
}
