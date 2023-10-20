using Common.DI;
using Microsoft.Extensions.Logging;

namespace JCorpus;

[AutoDiscover(AutoDiscoverOptions.Singleton)]
public class ConsoleCancellationSource
{
    public CancellationToken Token => cts.Token;

    public ConsoleCancellationSource(ILogger<ConsoleCancellationSource> logger)
    {
        this.logger = logger;
        Console.CancelKeyPress += OnCancel;
    }

    private void OnCancel(object? sender, ConsoleCancelEventArgs e)
    {
        logger.LogInformation("Canceling at next opportunity...");
        Console.CancelKeyPress -= OnCancel;
        cts.Cancel();
        e.Cancel = true;
    }

    private readonly CancellationTokenSource cts = new();
    private readonly ILogger<ConsoleCancellationSource> logger;
    private bool disposed = false;
}
