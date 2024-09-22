using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Content;

namespace Common.Addins.Extract;

public static class KnownFields
{
    public const string Title = nameof(Title);
    public const string Author = nameof(Author);
}

/// <summary>
/// A field which describes some resource. Only for UI purposes.
/// </summary>
/// <param name="Key">The key that identifies this field.</param>
/// <param name="Value">The value of the field.</param>
public record class ResourceField(
    string Key,
    string Value
);

/// <summary>
/// Details by which a new <see cref="CorpusWork"/> may be acquired.
/// May represent one volume of manga or novel, one chapter of a webnovel, etc.
/// </summary>
/// <param name="UniqueId">An arbitrary id that must uniquely identify this work in the corpus. Including a human-readable prefix is recommended.</param>
/// <param name="Uri">A URI representing the source of the work. May be an URL, a file path, etc.</param>
/// <param name="Fields">Fields that describe the resource. Will be displayed in the UI. See <see cref="KnownFields"/></param>
/// <param name="Tags">An optional list of tags that describe the resource kind, to allow extractors to select behavior.</param>
public record class CorpusWorkResource(
    CorpusWorkId UniqueId,
    string Uri,
    ResourceField[] Fields,
    string[] Tags
);