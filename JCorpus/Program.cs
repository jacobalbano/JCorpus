namespace JCorpus2;
using Common;
using Common.DI;
using Common.IO;
using JCorpus;
using JCorpus.Implementation;
using JCorpus.Implementation.IO.Filesystem;
using JCorpus.Persistence;
using JCorpus.Persistence.Models;
using JCorpus.Utility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Reflection;
using System.Text.Json;
using Utility;
using Utility.IO;

record class ProgramConstants(
    string WorkingDirectory
);

internal class Program
{
    private static JobParams GetJobConfiguration()
    {
        var embeddedFiles = new EmbeddedResourceFilesystem(typeof(Program).Assembly);
        var testParams = embeddedFiles.File("testParams.json");
        return JsonSerializer.Deserialize<JobParams>(testParams.ReadAllText());
    }

    static async Task Main(string[] args)
    {
        if (args.Any())
            Directory.SetCurrentDirectory(args[0]);

        var constants = new ProgramConstants(
            WorkingDirectory: WindowsPathUtility.MakeDirectoryPath(Directory.GetCurrentDirectory())
        );

        using var services = ConfigureServices(constants);
        var logger = services.GetRequiredService<ILogger<Program>>();
        var canceller = services.GetRequiredService<ConsoleCancellationSource>();
        var db = services.GetRequiredService<Database>();

        // TODO: get this from a POST or something
        var paramsFromWeb = GetJobConfiguration();

        using var jobScope = services.CreateScope();
        var enumerator = paramsFromWeb.Enumerator.Create(jobScope.ServiceProvider);

        var output = new VirtualFs(DirectoryPath.Combine(constants.WorkingDirectory, "output"));
        await foreach (var book in enumerator.GetAvailableWorks(canceller.Token))
        {
            if (canceller.Token.IsCancellationRequested)
                break;

            using var bookScope = services.CreateScope();
            using var dbSession = db.BeginSession();
            using var progress = new ExtractProgress(dbSession, book.UniqueId);
            var dbWork = dbSession.Select<DbCorpusWork>()
                .Where(x => x.CorpusWorkId == book.UniqueId)
                .FirstOrDefault() ?? new() { Status = Status.New, CorpusWorkId = book.UniqueId };
            try
            {
                switch (dbWork.Status)
                {
                    case Status.New:
                        dbSession.InsertOrUpdate(dbWork with { Status = Status.ExtractStarted });
                        break;
                    case Status.ExtractStarted:
                        break;
                    case Status.ExtractCompleted:
                        continue;
                    case Status.ExtractFailed:
                        logger.LogInformation("Extract previously failed; retrying");
                        progress.Reset();
                        dbWork = dbWork with { Status = Status.ExtractStarted };
                        break;
                    default:
                        throw new NotImplementedException($"Switch branch {dbWork.Status} not implemented");
                }

                logger.LogInformation("Processing book {uri}", book.Uri);
                var workOutput = output.Directory(book.UniqueId.ToString());
                workOutput.Create();

                var extractor = paramsFromWeb.Extractor.Create(bookScope.ServiceProvider);
                using var context = extractor.Extract(book, progress);
                using var entries = new CorpusEntryCollection(workOutput.File("@extract.txt"));
                await foreach (var entry in context.EnumerateEntries(canceller.Token))
                    entries.Add(entry);

                if (context is IFuriganaCollectionProvider ifp)
                    workOutput.File("furigana.txt").WriteAllLines(ifp.GetFuriganaCollection()
                        .Select(x => x.ToString()));

                if (context is IEvidenceProvider iep)
                    iep.WriteEvidenceToFolder(workOutput);

                if (canceller.Token.IsCancellationRequested)
                    break;

                dbWork = dbWork with { Status = Status.ExtractCompleted };
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error while extracting");
                dbWork = dbWork with { Status = Status.ExtractFailed };
            }
            finally
            {
                dbSession.Update(dbWork);
            }
        }
        
        logger.LogInformation("Done");
        Console.ReadLine();
    }

    static ServiceProvider ConfigureServices(ProgramConstants constants) =>
        new ServiceCollection()
        .AddSingleton(constants)
        .AddLogging(x => ConfigureLogging(x))
        .RunAutoDiscovery(CollectPluginTypes(constants.WorkingDirectory))
        .BuildServiceProvider()
        .ForceInitialization()
        ;

    private static ILoggingBuilder ConfigureLogging(ILoggingBuilder x) => x.AddSerilog(new LoggerConfiguration()
        .WriteTo.File("logs/jcorpus.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: null,
            shared: true
        )
        .WriteTo.Console()
        .CreateLogger());

    private static IReadOnlyList<Type> CollectPluginTypes(string root)
    {
        // TODO: don't hardcode this
        var paths = new[]
        {
            @"..\Plugins\CalibreLibrary\bin\Debug\net6.0\CalibreLibrary.dll",
            @"..\Plugins\Mokuro\bin\Debug\net6.0\Mokuro.dll",
        };

        var result = paths.Select(x => LoadPlugin(root, x))
            .SelectMany(x => x.GetTypes())
            .Concat(AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()))
            .Distinct()
            .ToList();

        return result;
    }

    static Assembly LoadPlugin(string root, string relativePath)
    {
        string pluginLocation = Path.GetFullPath(Path.Combine(root, relativePath.Replace('\\', Path.DirectorySeparatorChar)));
        PluginLoadContext loadContext = new PluginLoadContext(pluginLocation);
        return loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginLocation)));
    }
}