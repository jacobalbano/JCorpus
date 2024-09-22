using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.IO;

public static class VirtualFileUtility
{
    /// <summary>
    /// Open a text file, reads all lines of the file, and then closes the file.
    /// </summary>
    /// <param name="file">The file to read.</param>
    /// <returns>The file content as a list of lines.</returns>
    public static IEnumerable<string> ReadAllLines(this IVirtualFile file)
    {
        using var stream = file.Open(FileMode.Open);
        using var reader = new StreamReader(stream);

        string line = null;
        while ((line = reader.ReadLine()) != null)
            yield return line;
    }

    /// <summary>
    /// Open a text file, reads all contents, and then closes the file.
    /// </summary>
    /// <param name="file">The file to read.</param>
    /// <returns>The file content as a string.</returns>
    public static string ReadAllText(this IVirtualFile file)
    {
        using var stream = file.Open(FileMode.Open);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    // TODO: documentation

    public static void WriteAllLines(this IVirtualFile file, IEnumerable<string> lines)
    {
        using var stream = file.Open(FileMode.Create);
        using var writer = new StreamWriter(stream);
        foreach (var line in lines)
            writer.WriteLine(line);
    }

    public static void WriteAllText(this IVirtualFile file, string text)
    {
        using var stream = file.Open(FileMode.Create);
        using var writer = new StreamWriter(stream);
        writer.Write(text);
    }

    public static void WriteAllBytes(this IVirtualFile file, byte[] bytes)
    {
        using var stream = file.Open(FileMode.Create);
        using var writer = new BinaryWriter(stream);
        writer.Write(bytes);
    }

    public static void CopyTo(this IVirtualFile file, IVirtualFs directory, bool overwrite = false)
    {
        directory.Create();
        using var read = file.Open(FileMode.Open);
        using var write = directory.File(file.Filename).Open(overwrite ? FileMode.Create : FileMode.CreateNew);
        read.CopyTo(write);
    }
}
