using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common;

/// <summary>
/// Describes an object which can be frozen to prevent further modification.
/// </summary>
public interface IFreezable
{
    /// <summary>Whether this object has been frozen.</summary>
    bool Frozen { get; }

    /// <summary>
    /// Freeze this object to prevent further modification.<br />
    /// Implementations should use explicit definitions to reduce surface area.
    /// </summary>
    void Freeze();

    public static void ThrowIfFrozen(IFreezable obj)
    {
         if (obj.Frozen) throw new ObjectFrozenException();
    }
}

public class ObjectFrozenException : Exception
{
    public ObjectFrozenException() : base("Object cannot be modified after being frozen")
    {
    }
}
