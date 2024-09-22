using CsvHelper;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;
using System.Runtime.CompilerServices;
using Common.Addins.Extract;
using Common.IO;
using System.Text.Json;
using NodaTime;
using Common;
using Utility.IO;
using System.Net;
using Common.IO.Zip;
using Common.Configuration;
using System.Text.Json.Serialization;

namespace AozoraBunko;

public class Downloader : ICorpusWorkSource, IConfigurableWith<SourceConfig>
{
    public IReadOnlyList<string> OutTags => Constants.Tags;

    private readonly HttpClient client;
    private readonly IPersistentDirectory<Downloader> workingDirectory;
    private readonly ISystemClock clock;

    public SourceConfig Config { get; private set; }
    public void Configure(SourceConfig config) => Config = config;

    public Downloader(HttpClient client, IPersistentDirectory<Downloader> workingDirectory, ISystemClock clock)
    {
        this.client = client;
        this.workingDirectory = workingDirectory;
        this.clock = clock;
    }

    public Stream DownloadWork(string uri)
    {
        throw new NotImplementedException();
    }

    public async IAsyncEnumerable<CorpusWorkResource> EnumerateAvailableWorks([EnumeratorCancellation] CancellationToken ct)
    {
        var zip = await GetPersonListAsync(client, ct);
        var csv = zip.File("list_person_all.csv");

        using var entryStream = csv.Exists
            ? csv.Open(FileMode.Open)
            : throw new Exception("Failed to find CSV file in downloaded archive");

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        using var reader = new StreamReader(entryStream, Encoding.GetEncoding(932));
        using var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
        if (!csvReader.Read()) throw new Exception("Couldn't perform initial read of CSV file");
        if (!csvReader.ReadHeader()) throw new Exception("Couldn't read header of CSV file");

        int authorIdIndex = -1,
            authorNameIndex = -1,
            workIdIndex = -1,
            workNameIndex = -1;

        for (int i = 0; i < csvReader.HeaderRecord!.Length; i++)
        {
            switch (csvReader[i])
            {
                case "人物ID": authorIdIndex = i; break;
                case "著者名": authorNameIndex = i; break;
                case "作品ID": workIdIndex = i; break;
                case "作品名": workNameIndex = i; break;
                default: break;
            }
        }

        if (authorIdIndex == -1) throw new Exception($"Failed to find column index for {nameof(authorIdIndex)}");
        if (authorNameIndex == -1) throw new Exception($"Failed to find column index for {nameof(authorNameIndex)}");
        if (workIdIndex == -1) throw new Exception($"Failed to find column index for {nameof(workIdIndex)}");
        if (workNameIndex == -1) throw new Exception($"Failed to find column index for {nameof(workNameIndex)}");

        while (csvReader.Read())
        {
            var authorId = csvReader[authorIdIndex];
            var workId = csvReader[workIdIndex].TrimStart('0');
            var name = csvReader[workNameIndex].TrimStart('0');
            var authorName = csvReader[authorNameIndex].TrimStart();


            if (!(name.Contains(Config?.Title ?? string.Empty) && authorName.Contains(Config?.Author ?? string.Empty)))
                continue;

            yield return new CorpusWorkResource(
                $"aozora-{workId}",
                $"https://www.aozora.gr.jp/cards/{authorId}/card{workId}.html",
                new ResourceField[] {
                    new(KnownFields.Title, name),
                    new(KnownFields.Author, authorName),
                },
                Array.Empty<string>()
            );
        }
    }

    private async Task<IVirtualFs> GetPersonListAsync(HttpClient client, CancellationToken ct)
    {
        var file = workingDirectory.File("list_person_all.zip");
        if (!file.Exists || NeedsRedownload())
        {
            const string url = "https://www.aozora.gr.jp/index_pages/list_person_all.zip";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "JCorpus");

            var response = await client.SendAsync(request, ct);
            while (response.StatusCode == HttpStatusCode.Found)
            {
                request.RequestUri = response.Headers.Location;
                response = await client.SendAsync(request, ct);
            }

            using var stream = file.Open(FileMode.Create);
            using var zip = await response.Content.ReadAsStreamAsync(ct);
            await zip.CopyToAsync(stream, ct);
        }

        return new ZipFilesystem(new ZipArchive(file.Open(FileMode.Open)));
    }

    private bool NeedsRedownload()
    {
        var file = workingDirectory.File("lastUpdate.json");
        if (!file.Exists)
        {
            file.WriteAllText(JsonSerializer.Serialize(new LastUpdate(clock.GetCurrentInstant())));
            return true;
        }

        var text = file.ReadAllText();
        var lastUpdate = JsonSerializer.Deserialize<LastUpdate>(text);
        return clock.GetCurrentInstant() - lastUpdate.UpdateInstant > Duration.FromDays(7);
    }

    private record LastUpdate(
        [property: JsonConverter(typeof(NodaInstantJsonConverter))]
        Instant UpdateInstant
    );
}


internal class NodaInstantJsonConverter : JsonConverter<Instant>
{
    public override Instant Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return Instant.FromUnixTimeTicks(reader.GetInt64());
    }

    public override void Write(Utf8JsonWriter writer, Instant value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.ToUnixTimeTicks());
    }
}
