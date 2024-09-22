using CalibreLibrary.Metadata;
using Common;
using Common.Configuration;
using Common.DI;
using Common.IO;
using Common.Addins.Extract;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using Utility;
using Utility.IO;
using Common.Addins;
using CalibreLibrary.SQL;

namespace CalibreLibrary;

public class HostMachineSource : ICorpusWorkSource, IConfigurableWith<SourceConfig>
{
    public SourceConfig Config { get; private set; }
    void IConfigurableWith<SourceConfig>.Configure(SourceConfig config)
    {
        var oldDir = Config?.LibraryRoot;
        var newDir = config?.LibraryRoot;
        if (oldDir != newDir && newDir != null && !string.IsNullOrEmpty(newDir))
            libraryDir = fsAccess.Access(config.LibraryRoot);

        Config = config;
    }

    public IReadOnlyList<string> OutTags => new[] { "CBZ", "Epub" };

    public HostMachineSource(
        ILogger<HostMachineSource> logger,
        MetadataCache metadataCache,
        IFileSystemAccess<HostMachineSource> fsAccess,
        DbReader dbReader
    ){
        this.logger = logger;
        this.metadataCache = metadataCache;
        this.fsAccess = fsAccess;
        this.dbReader = dbReader;
    }

    public async IAsyncEnumerable<CorpusWorkResource> EnumerateAvailableWorks([EnumeratorCancellation] CancellationToken ct)
    {
        if (Config.IdentifierScheme == null)
        {
            logger.LogError("Required property {scheme} not supplied", nameof(Config.IdentifierScheme));
            yield break;
        }

        var books = dbReader.GetBookMetadata(libraryDir)
            .AsEnumerable();

        switch (Config.OrderBy?.Type ?? OrderByType.None)
        {
            case OrderByType.None:
                break;
            case OrderByType.Rating:
                books = (Config.OrderBy.Direction == OrderByDirection.Descending)
                    ? books.OrderByDescending(x => x.Value.Rating ?? 0)
                    : books.OrderBy(x => x.Value.Rating ?? 0);
                break;
            case OrderByType.Date:
                books = (Config.OrderBy.Direction == OrderByDirection.Descending)
                    ? books.OrderByDescending(x => x.Value.Timestamp)
                    : books.OrderBy(x => x.Value.Timestamp);
                break;
            default:
                logger.LogWarning("Unhandled OrderByType {type}", Config.OrderBy.Type);
                break;
        }

        if (Config.IdFilters?.Any() == true)
        {
            books = books.Where(x => x.Value.Identifiers
                .Where(bookId => Config.IdFilters.Any(filter => IdentifiersMatch(filter, bookId)))
                .Any());
        }

        if (Config.ExtensionFilters?.Any() == true)
        {
            books = books.Where(x => Config.ExtensionFilters
                .Any(y => string.Equals(x.Key.GetExtension(), y.ToString(), StringComparison.OrdinalIgnoreCase)));
        }

        foreach (var (path, metadata) in books.TakeWhile(_ => !ct.IsCancellationRequested))
        {
            var bookId = metadata.Identifiers?
                .FirstOrDefault(x => x.Scheme.Equals(Config.IdentifierScheme, StringComparison.InvariantCultureIgnoreCase));

            if (bookId == null)
            {
                logger.LogWarning("No {scheme} id found for {title}", Config.IdentifierScheme, path.GetUnqualifiedFilePath());
                continue;
            }

            yield return new CorpusWorkResource(
                bookId.Id,
                libraryDir.File(path).GetLocallyQualifiedPath(),
                new ResourceField[] {
                    new(KnownFields.Title, metadata.Title),
                    new(KnownFields.Author, metadata.Authors),
                    new("Rating", metadata.Rating.ToString()),
                },
                new[] { path.GetExtension().ToUpper() }
            );
        }
    }

    public Stream DownloadWork(string uri)
    {
        return libraryDir.File(uri)
            .Open(FileMode.Open);
    }

    private static bool IdentifiersMatch(Identifier filter, Identifier bookId)
    {
        return PathUtility.MatchGlob(bookId.Scheme, filter.Scheme)
            && PathUtility.MatchGlob(bookId.Id, filter.Id);
    }

    private readonly ILogger logger;
    private readonly MetadataCache metadataCache;
    private readonly IFileSystemAccess<HostMachineSource> fsAccess;
    private readonly DbReader dbReader;
    private IVirtualFs libraryDir;
}
