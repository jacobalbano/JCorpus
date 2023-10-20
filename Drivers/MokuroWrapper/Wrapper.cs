using Common.IO;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using Utility.IO;

namespace MokuroWrapper;

internal class Wrapper
{
    public static async Task Initialize(ILogger logger, IVirtualFs pythonHome)
    {
        var fs = new EmbeddedResourceFilesystem(typeof(Wrapper).Assembly);
        using (var stream = fs.File("Python/Python.zip").Open(FileMode.Open))
        using (var zip = new ZipArchive(stream))
        {
            if (!zip.Entries.Where(x => !x.FullName.EndsWith(IVirtualFs.PathSeparator))
                .All(x => pythonHome.File(x.FullName).Exists))
                zip.ExtractToDirectory(pythonHome.GetFullyQualifiedPath(), overwriteFiles: true);
        }

        var pythonPath = pythonHome.Directory("python-3.10.10-embed-amd64");

        var pipHome = pythonPath.Directory("Scripts");
        var pipLib = pythonPath.Directory("Lib");
        if (!pipHome.Exists || !pipLib.Exists)
        {
            var getPipPy = pythonHome.File("get-pip.py");
            var pythonExe = pythonPath.File("Python.exe");
            var pathDef = pythonPath.File("python310._pth");
            await InstallPip(logger, pythonExe, getPipPy, pathDef);
        }

        var mokuroPackage = pipLib.Directory("site-packages/mokuro");
        if (!mokuroPackage.Exists)
        {
            var pipExe = pipHome.File("pip3.exe");
            await InstallMokuro(logger, pipExe);
        }
    }

    private static async Task RunProcess(IVirtualFile exe, string arguments, ILogger logger = null, CancellationToken ct = default)
    {
        var process = new Process
        {
            StartInfo = new()
            {
                FileName = exe.GetFullyQualifiedPath(),
                Arguments = arguments,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                WorkingDirectory = Directory.GetCurrentDirectory(),
            },
            EnableRaisingEvents = true
        };

        if (logger != null)
        {
            process.OutputDataReceived += WrapLog(logger);
            process.ErrorDataReceived += WrapLog(logger);
        }

        process.Start();
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();

        using var unregister = ct.Register(() => process.Kill());
        await process.WaitForExitAsync(CancellationToken.None);
    }

    private static DataReceivedEventHandler WrapLog(ILogger logger)
    {
        return (object sender, DataReceivedEventArgs o) =>
        {
            var message = o.Data;
            if (string.IsNullOrWhiteSpace(message)) return;
            if (message.Contains("Processing pages...")) return;

            int index = message.IndexOf('|');
            if (index >= 0) index = message.IndexOf('|', index);
            if (index >= 0) index = message.IndexOf(" - ", index);
            if (index >= 0) message = message[(index + 3)..];

            if (message.Contains("using cpu", StringComparison.OrdinalIgnoreCase))
                logger.LogWarning("Mokuro: using CPU! Consider installing a GPU-compatible version of pytorch");
            else if (message.Contains("Processed successfully: 0/1", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Extract failed");
            else
                logger.LogInformation("Mokuro: {message}", message);
        };
    }

    public static Task Run(ILogger logger, IVirtualFs pythonHome, string path, CancellationToken ct)
    {
        var pythonExe = pythonHome.File("python-3.10.10-embed-amd64/Python.exe");
        return RunProcess(
            pythonExe,
            $"-m mokuro --disable-confirmation true \"{path}\"",
            logger,
            ct
        );
    }

    private static async Task InstallMokuro(ILogger logger, IVirtualFile pipExe)
    {
        await RunProcess(pipExe, "install mokuro", logger);
        await RunProcess(pipExe, "uninstall -y torch torchvision torchaudio", logger);
        await RunProcess(pipExe, "cache purge", logger);
        await RunProcess(pipExe, "install torch torchvision torchaudio --extra-index-url https://download.pytorch.org/whl/cu117", logger);
    }

    private static async Task InstallPip(ILogger logger, IVirtualFile pythonExe, IVirtualFile getPipPy, IVirtualFile pythonPathFile)
    {
        await RunProcess(pythonExe, getPipPy.GetFullyQualifiedPath(), logger);
        var lines = pythonPathFile.ReadAllLines()
            .Concat(appendPipPaths)
            .Distinct()
            .ToList();

        pythonPathFile.WriteAllLines(lines);
    }

    private static readonly string[] appendPipPaths = new[]
    {
        "Lib",
        "Lib/site-packages"
    };
}