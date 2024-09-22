using CalibreLibrary.Metadata;
using Common.DI;
using Common.IO;
using Microsoft.Extensions.Logging;
using NodaTime;
using NodaTime.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using Utility.IO;

namespace CalibreLibrary;

[AutoDiscover(AutoDiscoverOptions.Scoped)]
public class MetadataCache
{
    public MetadataCache(IPersistentDirectory<MetadataCache> cacheDir, ILogger<MetadataCache> logger)
    {
        this.cacheDir = cacheDir;
        this.logger = logger;
    }

    public IReadOnlyDictionary<FilePath, BookMetadata> GetMetadataForLibrary(IVirtualFs libraryRoot)
    {
        var pathKey = libraryRoot.DirectoryName.GetPathParts()
            .Last().ToString(); // TODO: might still cause collisions

        if (!libraries.TryGetValue(pathKey, out var cache))
            libraries.Add(pathKey, cache = EstablishCache(pathKey, libraryRoot));

        return cache;
    }

    private Dictionary<FilePath, BookMetadata> EstablishCache(string pathKey, IVirtualFs libraryRoot)
    {
        var cacheFile = cacheDir.File(pathKey + ".json");

        var cache = cacheFile.Exists
            ? JsonSerializer.Deserialize<Dictionary<FilePath, BookMetadata>>(cacheFile.ReadAllText())
            : new();

        foreach (var path in libraryRoot.EnumerateFiles("metadata.opf", SearchOption.AllDirectories))
        {
            var metapath = libraryRoot.File(path);
            var bookFolder = metapath.ContainingDirectory;
            var books = bookFolder.EnumerateFiles()
                .Where(x => extensions.Contains(x.GetExtension()))
                .Select(x => metapath.ContainingDirectory.File(x).GetLocallyQualifiedPath())
                .Where(x => !calibreSystemDirs.Contains(x.GetPathParts().First()))
                .ToList();

            if (!books.Any(x => !cache.ContainsKey(x)))
                continue;

            XNamespace opf = "http://www.idpf.org/2007/opf";
            XNamespace dc = "http://purl.org/dc/elements/1.1/";
            var metaFile = XElement.Parse(metapath.ReadAllText());

            var metadata = metaFile.Element(opf + "metadata");
            if (metadata == null)
            {
                logger.LogError("Badly formed metadata file at {metapath}", metapath.GetFullyQualifiedPath());
                continue;
            }

            var timestamp = metadata
                .Elements(opf + "meta")
                .Where(x => x.Attribute("name")?.Value == "calibre:timestamp")
                .Select(x => x.Attribute("content")?.Value)
                .Where(x => x != null)
                .Select(x => OffsetDateTimePattern.ExtendedIso.Parse(x))
                .Where(x => x.Success)
                .Select(x => x.Value.ToInstant())
                .FirstOrDefault();

            if (timestamp == default)
            {
                logger.LogError("Missing or malformed timestamp in {metapath}", metapath.GetFullyQualifiedPath());
                continue;
            }

            var identifiers = metadata
                .Elements(dc + "identifier")
                .Select(x => new Identifier(x.Attribute(opf + "scheme")?.Value!, x.Value))
                .Where(x => x.Scheme != null)
                .ToList();

            var title = metadata
                .Elements(dc + "title")
                .Select(x => x.Value)
                .FirstOrDefault();

            var rating = metadata
                .Elements(opf + "meta")
                .Where(x => x.Attribute("name")?.Value == "calibre:rating")
                .Select(x => x.Attribute("content")?.Value)
                .Where(x => x != null)
                .Select(x => byte.Parse(x))
                .FirstOrDefault();

            foreach (var bookFile in books)
            {
                if (!extensions.Contains(bookFile.GetExtension()))
                    continue;

                //cache[bookFile] = new BookMetadata(
                //    rating == 0 ? null : rating,
                //    title ?? "Untitled",
                //    identifiers,
                //    timestamp
                //);
            }
        }

        cacheFile.WriteAllText(JsonSerializer.Serialize(cache));
        return cache;
    }

    private readonly Dictionary<string, Dictionary<FilePath, BookMetadata>> libraries = new();
    private readonly IPersistentDirectory<MetadataCache> cacheDir;
    private readonly ILogger logger;
    private static readonly HashSet<string> extensions = new() { "cbz", "epub" };
    private static readonly HashSet<string> calibreSystemDirs = new() { ".caltrash", ".calnotes" };
}
