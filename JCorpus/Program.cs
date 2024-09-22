using Common;
using Common.DI;
using Common.Addins.Extract;
using JCorpus;
using JCorpus.DI;
using JCorpus.Implementation;
using JCorpus.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Diagnostics;
using System.Reflection;
using Utility.IO;
using Common.Addins;
using JCorpus.Implementation.IO.Filesystem;
using System.Text.Json;
using Common.IO;
using Common.Content;

[assembly: AutoDiscoverAssembly]

namespace JCorpus;

record class ProgramConstants(
    string WorkingDirectory
);

public record class JCorpus(
    string Author = "Jacob Albano",
    string Description = "Obtain, index, and search Japanese text from a variety of sources"
) : IPlugin<JCorpus>;

internal class Program
{
    static void Main(string[] args)
    {
        if (args.Any())
            Directory.SetCurrentDirectory(args[0]);

        var constants = new ProgramConstants(
            WorkingDirectory: WindowsPathUtility.MakeDirectoryPath(Directory.GetCurrentDirectory())
        );

        var perf = Stopwatch.StartNew();
        using var services = ConfigureServices(constants);
        var canceller = services.GetRequiredService<ConsoleCancellationSource>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        var host = services.GetRequiredService<WebserviceHost>();
        host.Start();

        logger.LogInformation("Service start took {time:0.00}s", perf.Elapsed.TotalSeconds);
        canceller.Token.WaitHandle.WaitOne();
        host.Stop();
    }

    private class Handler : HttpClientHandler
    {
        public Handler()
        {
            AllowAutoRedirect = false;
        }
    }

    static ServiceProvider ConfigureServices(ProgramConstants constants) =>
        new ServiceCollection()
        .AddSingleton(new HttpClient(new Handler()))
        .AddSingleton(constants)
        .AddLogging(x => ConfigureLogging(x))
        .RunAutoDiscovery(CollectPluginTypes(constants.WorkingDirectory))
        .BuildServiceProvider()
        .ForceInitialization()
        ;

    private static ILoggingBuilder ConfigureLogging(ILoggingBuilder x) =>
        x.AddSerilog(new LoggerConfiguration()
        .WriteTo.File("logs/jcorpus.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: null,
            shared: true,
            outputTemplate: OutputTemplate
        )
#if DEBUG
        .MinimumLevel.Is(Serilog.Events.LogEventLevel.Debug)
#endif
        .WriteTo.Console(outputTemplate: OutputTemplate)
        .CreateLogger());

    private static IReadOnlyList<Type> CollectPluginTypes(string root)
    {
        // TODO: make this configurable
        return Directory.EnumerateFiles(@"..\Plugins\", "*.dll", SearchOption.AllDirectories)
            .Select(x => (success: TryLoadPlugin(root, x, out var asm), asm))
            .Where(x => x.success)
            .SelectMany(x => x.asm.ExportedTypes)
            .Concat(AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()))
            .DistinctBy(x => x.GUID)
            .ToList();
    }

    static bool TryLoadPlugin(string root, string relativePath, out Assembly asm)
    {
        try
        {
            string pluginLocation = Path.GetFullPath(Path.Combine(root, relativePath.Replace('\\', Path.DirectorySeparatorChar)));
            var loadContext = new PluginLoadContext(pluginLocation);
            asm = loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginLocation)));
            return true;
        }
        catch (Exception)
        {
            asm = null;
            return false;
        }
    }

    private const string OutputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3} {SourceContext:1}] {Message:lj}{NewLine}{Exception}";
}