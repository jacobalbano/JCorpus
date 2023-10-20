using System.Text.RegularExpressions;

namespace Common.Content;

/// <summary>
/// Strongly-typed string Id. Provides name validation. May be unique within the corpus or within a <see cref="CorpusWork"/>.
/// </summary>
public readonly record struct UniqueId
{
    public string Value { get; }

    public static implicit operator UniqueId(string value) => new(value);
    public static implicit operator string(UniqueId id) => id.Value;
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;

    public UniqueId(string id)
    {
        if (validate.IsMatch(id))
            throw new Exception("Id contains invalid characters");
        Value = id;
    }

    private static readonly Regex validate = new(@"[\r\n|\s]", RegexOptions.Compiled);
}