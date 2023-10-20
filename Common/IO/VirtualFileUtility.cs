using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.IO;

public static class VirtualFileUtility
{
    public static IEnumerable<string> ReadAllLines(this IVirtualFile file)
    {
        using var stream = file.Open(FileMode.Open);
        using var reader = new StreamReader(stream);

        string? line = null;
        while ((line = reader.ReadLine()) != null)
            yield return line;
    }

    public static string ReadAllText(this IVirtualFile file)
    {
        using var stream = file.Open(FileMode.Open);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

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
}
