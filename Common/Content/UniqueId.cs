using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Common.Content;

public abstract record class UniqueIdBase<T> : IComparable<T>, IFormattable where T : UniqueIdBase<T>
{
    private string Value
    {
        get => value;
        init
        {
            if (validate.IsMatch(value))
                throw new Exception("Id contains invalid characters");
            this.value = value;
        }
    }

    int IComparable<T>.CompareTo(T obj) => Value.CompareTo(obj.Value);
    public UniqueIdBase(string value) => Value = value;
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;
    string IFormattable.ToString(string format, IFormatProvider formatProvider) => Value;

    private readonly string value;

    private static readonly Regex validate = new(@"[\r\n|\s]", RegexOptions.Compiled);

    protected abstract class ConverterBase : JsonConverter<T>
    {
        protected abstract T Create(string value);

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => Create(reader.GetString());

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.Value);
    }
}