namespace Common.IO;

/// <summary>
/// Defines behavior common to both <see cref="IVirtualFile"/> and <see cref="IVirtualFs"/>.
/// </summary>
public interface IVirtualFsObject
{
    /// <summary>
    /// The <see cref="IVirtualFs"/> that directly contains this object.
    /// May be null if this object represents a directory root.
    /// </summary>
    IVirtualFs ContainingDirectory { get; }

    /// <summary>
    /// Whether this virtual filesystem is read-only. If true, create/update/delete operations should throw a <see cref="ReadOnlyFilesystemException"/>
    /// </summary>
    bool IsReadOnly { get; }

    /// <summary>
    /// Whether this object represents an existing path.
    /// </summary>
    bool Exists { get; }

    /// <summary>Delete the filesystem entry that this object represents, if it exists.</summary>
    /// <exception cref="ReadOnlyFilesystemException">If the filestem is read-only, this exception will be thrown.</exception>
    void Delete();
}