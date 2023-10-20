using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.IO;

/// <summary>
/// Represents a path to a file.
/// All paths use the forward slash as a path separator.
/// File paths cannot end in the path separator.
/// </summary>
[JsonConverter(typeof(JsonConverter))]
public readonly record struct FilePath
{
    /// <summary>
    /// Combine a directory path with a file path.
    /// </summary>
    /// <param name="dirPath">The directory path.</param>
    /// <param name="filePath">The file path.</param>
    /// <returns>The resulting qualified filepath.</returns>
    public static FilePath Combine(DirectoryPath dirPath, FilePath filePath)
    {
        return string.Join(IVirtualFs.PathSeparator, new string[2]
        {
            dirPath,
            filePath
        });
    }

    public FilePath(string path) => PathUtility.ThrowOnInvalidPath(this.path = path);
    public static implicit operator FilePath(string path) => new(path);
    public static implicit operator string(FilePath path) => path.ToString();
    public override readonly string ToString() => path;
    public static bool operator == (string str, FilePath path) => path.path == str;
    public static bool operator != (string str, FilePath path) => path.path != str;
    private readonly string path;

    private class JsonConverter : JsonConverter<FilePath>
    {
        public override FilePath ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.GetString();
        public override void WriteAsPropertyName(Utf8JsonWriter writer, FilePath value, JsonSerializerOptions options) => writer.WritePropertyName(value);

        public override FilePath Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.GetString();
        public override void Write(Utf8JsonWriter writer, FilePath value, JsonSerializerOptions options) => writer.WriteStringValue(value);
    }
}