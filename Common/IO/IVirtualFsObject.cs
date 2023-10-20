namespace Common.IO;

public interface IVirtualFsObject
{
    /// <summary>
    /// The <see cref="IVirtualFs"/> that contains this object.
    /// May be null if this object represents a directory root.
    /// </summary>
    IVirtualFs? ContainingDirectory { get; }

    /// <summary>
    /// Whether this virtual filesystem is writeable.
    /// </summary>
    bool IsReadOnly { get; }

    /// <summary>
    /// Whether this object represents an existing path.
    /// </summary>
    bool Exists { get; }

    /// <summary>Delete the filesystem entry that this object represents, if it exists.</summary>
    void Delete();
}