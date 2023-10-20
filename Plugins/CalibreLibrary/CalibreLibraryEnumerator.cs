using CalibreLibrary.Metadata;
using Common;
using Common.Content;
using Common.DI;
using Common.IO;
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

namespace CalibreLibrary;

public class CalibreLibraryEnumerator : ICorpusWorkEnumerator, IConfigurable<EnumeratorConfig>
{
    public CalibreLibraryEnumerator(ILogger<CalibreLibraryEnumerator> logger, IConfigProvider<EnumeratorConfig> configProvider, CalibreMetadataCache metadataCache)
    {
        this.logger = logger;
        this.metadataCache = metadataCache;
        config = configProvider.Get();
    }

    public async IAsyncEnumerable<CorpusWork> GetAvailableWorks([EnumeratorCancellation] CancellationToken ct)
    {
        var books = metadataCache.GetMetadataForLibrary(config.LibraryRoot)
            .AsEnumerable();

        if (config.OrderBy != null)
        {
            switch (config.OrderBy.Type)
            {
                case OrderByType.None:
                    break;
                case OrderByType.Rating:
                    books = (config.OrderBy.Direction == OrderByDirection.Descending)
                        ? books.OrderByDescending(x => x.Value.Rating ?? 0)
                        : books.OrderBy(x => x.Value.Rating ?? 0);
                    break;
                case OrderByType.Date:
                    books = (config.OrderBy.Direction == OrderByDirection.Descending)
                        ? books.OrderByDescending(x => x.Value.Timestamp)
                        : books.OrderBy(x => x.Value.Timestamp);
                    break;
                default:
                    logger.LogWarning("Unhandled OrderByType {type}", config.OrderBy.Type);
                    break;
            }
        }

        if (config.IdFilters != null)
        {
            books = books.Where(x =>
            {
                var matches = x.Value.Identifiers
                    .Where(bookId => config.IdFilters.Any(filter => IdentifiersMatch(filter, bookId)))
                    .ToList();

                return matches.Any();
            });
        }

        if (config.ExtensionFilters != null)
        {
            books = books.Where(x => config.ExtensionFilters.Contains(x.Key.GetExtension()));
        }

        foreach (var (path, metadata) in books)
        {
            if (ct.IsCancellationRequested)
                yield break;

            var cmoaId = metadata.Identifiers?
                .FirstOrDefault(x => x.Scheme.Equals("cmoa", StringComparison.InvariantCultureIgnoreCase));

            if (cmoaId == null)
            {
                logger.LogWarning("No cmoa ID found for {title}", path.GetUnqualifiedFilePath());
                continue;
            }

            yield return new CorpusWork(
                cmoaId.Id,
                config.LibraryRoot.File(path).GetFullyQualifiedPath()
            );
        }
    }

    private static bool IdentifiersMatch(Identifier filter, Identifier bookId)
    {
        return PathUtility.MatchGlob(bookId.Scheme, filter.Scheme)
            && PathUtility.MatchGlob(bookId.Id, filter.Id);
    }

    private readonly ILogger logger;
    private readonly CalibreMetadataCache metadataCache;
    private readonly EnumeratorConfig config;
}
