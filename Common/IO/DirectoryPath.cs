using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace Common.IO;

/// <summary>
/// Represents a path to a directory.
/// All paths use the forward slash as a path separator.
/// </summary>
[JsonConverter(typeof(JsonConverter))]
public readonly record struct DirectoryPath
{
    /// <summary>
    /// Combine an array of paths into one.
    /// </summary>
    /// <param name="paths">The paths to combine.</param>
    /// <returns>The combined path.</returns>
    /// <exception cref="Exception">If the path array is empty.</exception>
    public static DirectoryPath Combine(params DirectoryPath[] paths)
    {
        if (paths.Length < 1) throw new Exception("PathUtility.Combine requires at least one path");
        return string.Join(IVirtualFs.PathSeparator, paths);
    }

    public DirectoryPath(string path) => PathUtility.ThrowOnInvalidPath(this.path = path);
    public static implicit operator DirectoryPath(string path) => new(path);
    public static implicit operator string(DirectoryPath path) => path.ToString();
    public override readonly string ToString() => path;

    private readonly string path;

    private class JsonConverter : JsonConverter<DirectoryPath>
    {
        public override DirectoryPath ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.GetString();
        public override void WriteAsPropertyName(Utf8JsonWriter writer, DirectoryPath value, JsonSerializerOptions options) => writer.WritePropertyName(value);

        public override DirectoryPath Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.GetString();
        public override void Write(Utf8JsonWriter writer, DirectoryPath value, JsonSerializerOptions options) => writer.WriteStringValue(value);
    }
}
