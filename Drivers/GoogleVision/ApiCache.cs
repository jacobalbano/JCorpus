using Google.Cloud.Vision.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GoogleVision;

internal class ApiCache
{
    public static ApiCache Instance { get; } = new();

    private ApiCache()
    {
        var builder = new ImageAnnotatorClientBuilder();
        builder.JsonCredentials = File.ReadAllText("GoogleVision/credentials.json");
        client = builder.Build();

        Directory.CreateDirectory("GoogleVision/cache");
    }

    public async Task<OcrResult> DetectText(string corpusWorkUid, string imageLocalId, Stream imageStream)
    {
        var filename = Path.Combine("GoogleVision/cache", GetResultsFilename(corpusWorkUid, imageLocalId));
        if (File.Exists(filename))
            return JsonSerializer.Deserialize<OcrResult>(File.ReadAllText(filename));

        try
        {
            var image = await Image.FromStreamAsync(imageStream);
            var detectResult = await client.DetectDocumentTextAsync(image, new ImageContext { LanguageHints = { "ja" } });
            var blocks = detectResult?.Pages.FirstOrDefault()?.Blocks ?? emptyBlocks;
            var result = new OcrResult(blocks
                .Where(x => x.BlockType == Block.Types.BlockType.Text)
                .Select(x => TextBlock.From(x)).ToList()
            );

            File.WriteAllText(filename, JsonSerializer.Serialize(result, options: new JsonSerializerOptions { WriteIndented = true }));

            return result;
        }
        finally
        {
            Interlocked.Increment(ref requests);
        }
    }

    private volatile int requests = 0;
    private readonly ImageAnnotatorClient client;
    private static string GetResultsFilename(string workId, string imageId) => $"{workId}-{imageId}.json";
    private static readonly IEnumerable<Block> emptyBlocks = Array.Empty<Block>();
}
