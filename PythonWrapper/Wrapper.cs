using System.Diagnostics;
using System.Reflection;

namespace PythonWrapper;

public class Wrapper
{
    public static void Initialize()
    {
        Console.WriteLine("Installing pip");
        var asmLocation = new FileInfo(Assembly.GetExecutingAssembly()
            .Location)
            .DirectoryName;

        var installPip = new Process()
        {
            StartInfo = new()
            {
                FileName = pythonExe,
                Arguments = Path.Combine(pythonDeps, "get-pip.py"),
                CreateNoWindow = true,
                WorkingDirectory = asmLocation,
                UseShellExecute = false,
                RedirectStandardOutput = true,
            }
        };

        installPip.Start();
        while (!installPip.StandardOutput.EndOfStream)
            Console.WriteLine(installPip.StandardOutput.ReadLine());
        installPip.WaitForExit();

        var lines = File.ReadAllLines(pythonPathDef)
            .ToList();

        foreach (var path in appendPipPaths.Except(lines))
                lines.Add(path);

        File.WriteAllLines(pythonPathDef, lines);
        Console.ReadLine();
    }

    const string pythonDeps = "Python";
    const string pythonPath = $"{pythonDeps}/python-3.10.10-embed-amd64";
    const string pythonExe = $"{pythonPath}/Python.exe";
    const string pythonPathDef = $"{pythonPath}/python310._pth";

    private static readonly string[] appendPipPaths = new[]
    {
        "Lib",
        "Lib/site-packages"
    };
}