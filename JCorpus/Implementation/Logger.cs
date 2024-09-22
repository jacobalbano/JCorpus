using Common.DI;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCorpus.Implementation;

[AutoDiscover(AutoDiscoverOptions.Singleton, ImplementationFor = typeof(ILogger<>))]
internal class Logger<T> : ILogger<T>
{
    public Logger(ILoggerFactory factory)
    {
        if (factory == null)
            throw new ArgumentNullException(nameof(factory));

        _logger = factory.CreateLogger($"{typeof(T).Assembly.GetName().Name}/{GetNameWithoutGenericArity(typeof(T))}");
    }

    IDisposable ILogger.BeginScope<TState>(TState state) => _logger.BeginScope(state);

    bool ILogger.IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

    void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        => _logger.Log(logLevel, eventId, state, exception, formatter);

    private readonly ILogger _logger;

    private static string GetNameWithoutGenericArity(Type t)
    {
        string name = t.Name;
        int index = name.IndexOf('`');
        return index == -1 ? name : name[..index];
    }
}