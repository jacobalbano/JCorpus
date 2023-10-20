using Common.IO;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MokuroWrapper;

public sealed class MokuroProcess
{
    public MokuroProcess(IVirtualFs ocrDir, IVirtualFs processDirectory, IVirtualFs pythonHome, ILogger logger)
    {
        this.ocrDir = ocrDir;
        this.processDirectory = processDirectory;
        this.logger = logger;
        this.pythonHome = pythonHome;
    }

    public async IAsyncEnumerable<MokuroJsonFile> YieldJsonFiles([EnumeratorCancellation]CancellationToken ct)
    {
        await Initializer.Run(logger, pythonHome);
        using var watcher = new FileSystemWatcher(ocrDir.GetFullyQualifiedPath())
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.FileName,
            Filter = "*.json",
            EnableRaisingEvents = true,
        };

        var channel = Channel.CreateUnbounded<FilePath>();
        watcher.Created += (_, e) => channel.Writer.TryWrite(WindowsPathUtility.MakeFilePath(e.Name));

        _ = Task.Run(async () => {
            await Wrapper.Run(logger, pythonHome, processDirectory.GetFullyQualifiedPath(), ct);
            channel.Writer.Complete();
        }, CancellationToken.None);

        await foreach (var path in channel.Reader.ReadAllAsync(CancellationToken.None))
        {
            if (ct.IsCancellationRequested)
                yield break;

            var file = ocrDir.File(path);
            var json = JsonSerializer.Deserialize<MokuroJson>(await TryReadContestedFile(file))!;
            yield return new(path, json);
        }

        await Task.Delay(1000, CancellationToken.None);
    }

    private readonly ILogger logger;
    private readonly IVirtualFs ocrDir;
    private readonly IVirtualFs processDirectory;
    private readonly IVirtualFs pythonHome;

    private static async Task<string> TryReadContestedFile(IVirtualFile file)
    {
        for (int tries = 0; tries < 10; ++tries)
        {
            try
            {
                return file.ReadAllText();
            }
            catch (IOException)
            {
                if (tries == 9)
                    throw;

                await Task.Delay(300);
            }
        }

        throw new Exception("Unreachable code");
    }

    private static class Initializer
    {
        public static async Task Run(ILogger logger, IVirtualFs pythonHome)
        {

            await initializeSemaphore.WaitAsync();
            if (!mokuroInitialized)
            {
                logger.LogInformation("Initializing mokuro");
                await Wrapper.Initialize(logger, pythonHome);
                logger.LogInformation("Mokuro initialized");
                mokuroInitialized = true;
            }

            initializeSemaphore.Release();
        }

        private static volatile bool mokuroInitialized = false;
        private static readonly SemaphoreSlim initializeSemaphore = new(1);
    }
}
