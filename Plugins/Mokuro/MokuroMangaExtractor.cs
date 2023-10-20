
using Common;
using Common.Content;
using Common.IO;
using Common.Utility;
using Microsoft.Extensions.Logging;
using MokuroWrapper;
using SkiaSharp;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using Utility;

namespace Mokuro;

public class MokuroMangaExtractor : ICorpusWorkExtractor
{
    public MokuroMangaExtractor(ITempDirectory tempDir, ILogger<MokuroMangaExtractor> logger, IPersistentDirectory<PythonHome> pythonHome)
    {
        this.logger = logger;
        this.pythonHome = pythonHome;

        this.tempDir = tempDir;
        mokuroDir = tempDir.Directory("mokuro");
        ocrDir = tempDir.Directory("_ocr/mokuro");

        mokuroDir.Create();
        ocrDir.Create();
    }

    public ICorpusWorkExtractorContext Extract(CorpusWork work, IExtractProgress progress)
    {

        logger.LogInformation("Unzipping");
        //TODO: this breaks the abstraction. should introduce a downloader pipeline stage that supplies a file stream
        ZipFile.ExtractToDirectory(work.Uri, mokuroDir.GetFullyQualifiedPath(), true);
        logger.LogInformation("Finished unzip");

        var existingImages = mokuroDir.EnumerateFiles()
            .ToDictionary(x => x.GetFilenameWithoutExtension());

        foreach (var id in progress.GetIds())
        {
            if (existingImages.TryGetValue(id, out var path))
            {
                logger.LogInformation("Deleting file {path} to fast-forward mokuro extract", path);
                mokuroDir.File(path).Delete();
            }
        }

        return new ExtractorContextImpl(ocrDir, mokuroDir, logger, pythonHome, progress);
    }

    private class ExtractorContextImpl : ICorpusWorkExtractorContext, IEvidenceProvider
    {
        public ExtractorContextImpl(IVirtualFs ocrDir, IVirtualFs mokuroDir, ILogger<MokuroMangaExtractor> logger, IPersistentDirectory<PythonHome> pythonHome, IExtractProgress progress)
        {
            this.ocrDir = ocrDir;
            this.mokuroDir = mokuroDir;
            this.logger = logger;
            this.pythonHome = pythonHome;
            this.progress = progress;
        }

        void IDisposable.Dispose()
        {
        }

        public async IAsyncEnumerable<CorpusEntry> EnumerateEntries([EnumeratorCancellation] CancellationToken ct)
        {
            logger.LogInformation("Running mokuro");
            var process = new MokuroProcess(ocrDir, mokuroDir, pythonHome, logger);
            await foreach (var (path, json) in process.YieldJsonFiles(ct))
            {
                if (ct.IsCancellationRequested)
                    yield break;

                var page = path.GetFilenameWithoutExtension();
                logger.LogInformation("Processing page {page}", page);

                foreach (var (block, blockNum) in json.Blocks.Select((x, i) => (x, i)))
                {
                    var scopedId = $"{page}-{blockNum}";
                    EvidenceImages.Add(new(path, scopedId, block.Box));
                    EvidenceJson.Add(ocrDir.File(path));
                    yield return new CorpusEntry(scopedId, string.Join("", block.Lines));
                }

                progress.ReportProgress(page);
            }
        }

        public void WriteEvidenceToFolder(IVirtualFs outputFolder)
        {
            var files = mokuroDir.EnumerateFiles("*", SearchOption.AllDirectories)
                .Where(x => imageExtensions.Contains(x.GetExtension()))
                .ToDictionary(ReplaceExtensionWithJson);

            SKBitmap bitmap = null;
            string lastPath = null;
            foreach (var (path, id, box) in EvidenceImages)
            {
                if (!files.TryGetValue(path, out var imagePath))
                    continue; // TODO: warn

                if (lastPath != imagePath)
                {
                    bitmap?.Dispose();
                    using var stream = mokuroDir.File(imagePath).Open(FileMode.Open);
                    lastPath = imagePath;
                    bitmap = SKBitmap.Decode(stream);
                }

                using var pixmap = new SKPixmap(bitmap.Info, bitmap.GetPixels());
                var subset = pixmap.ExtractSubset(new(
                    (int)box.TopLeft.X,
                    (int)box.TopLeft.Y,
                    (int)box.BottomRight.X,
                    (int)box.BottomRight.Y
                ));

                using var data = subset.Encode(SKJpegEncoderOptions.Default);
                outputFolder.File($"{id}.jpg").WriteAllBytes(data.ToArray());
            }

            bitmap?.Dispose();

            foreach (var file in EvidenceJson)
            {
                var outFile = outputFolder.File(file.Filename);
                outFile.WriteAllText(file.ReadAllText());
            }
        }

        private static FilePath ReplaceExtensionWithJson(FilePath path)
        {
            var directoryParts = path.GetPathParts().ToArray();
            var filename = path.GetFilenameWithoutExtension();
            FilePath jsonFilepath = filename + ".json";
            if (directoryParts.Any())
                return FilePath.Combine(DirectoryPath.Combine(directoryParts), jsonFilepath);
            
            return jsonFilepath;
        }

        private readonly List<Evidence> EvidenceImages = new();
        private readonly List<IVirtualFile> EvidenceJson = new();
        private readonly IVirtualFs ocrDir;
        private readonly IVirtualFs mokuroDir;
        private readonly ILogger<MokuroMangaExtractor> logger;
        private readonly IPersistentDirectory<PythonHome> pythonHome;
        private readonly IExtractProgress progress;

        private record Evidence(FilePath JsonFilePath, string ScopedId, MokuroBoundingBox Box);
        private static readonly HashSet<string> imageExtensions = new(StringComparer.InvariantCultureIgnoreCase) { "jpg", "png", "jpeg" };
    }

    private readonly ITempDirectory tempDir;
    private readonly ILogger<MokuroMangaExtractor> logger;
    private readonly IPersistentDirectory<PythonHome> pythonHome;
    private IVirtualFs mokuroDir;
    private IVirtualFs ocrDir;

    public class PythonHome { private PythonHome() { } }
}
