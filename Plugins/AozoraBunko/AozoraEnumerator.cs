using CsvHelper;
using Common;
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
using Common.Content;

namespace AozoraBunko;

public class AozoraEnumerator : ICorpusWorkEnumerator
{
    private readonly HttpClient client;
    private readonly IPersistentDirectory<AozoraEnumerator> workingDirectory;

    public AozoraEnumerator(HttpClient client, IPersistentDirectory<AozoraEnumerator> workingDirectory)
    {
        this.client = client;
        this.workingDirectory = workingDirectory;
    }

    public async IAsyncEnumerable<CorpusWork> GetAvailableWorks([EnumeratorCancellation] CancellationToken ct)
    {
        var file = workingDirectory.File("list_person_all.zip");
        using var stream = file.Exists
            ? file.Open(FileMode.Open)
            : await client.GetStreamAsync("https://www.aozora.gr.jp/index_pages/list_person_all.zip", ct);

        using var zip = new ZipArchive(stream);
        var csvEntry = zip.Entries.FirstOrDefault(x => x.Name.EndsWith(".csv"))
            ?? throw new Exception("Failed to find CSV file in downloaded archive");

        using var entryStream = csvEntry.Open();
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
            var workId = csvReader[workIdIndex]!.TrimStart('0');
            yield return new CorpusWork(workId, $"https://www.aozora.gr.jp/cards/{authorId}/card{workId}.html");
        }
    }
}
