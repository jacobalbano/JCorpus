
using Common;
using Common.Content;
using Common.IO;
using Common.Addins.Extract;
using Common.Utility;
using Microsoft.Extensions.Logging;
using MokuroWrapper;
using SkiaSharp;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using Utility.IO;

namespace Mokuro;

public class Extractor : ICorpusWorkExtractor
{
    public IReadOnlyList<string> MetadataTags => new[] { KnownTags.OCR };

    public IReadOnlyList<string> InTags => new[] { "CBZ" };

    public Extractor(ITempDirectory tempDir, ILogger<Extractor> logger, IPersistentDirectory<PythonHome> pythonHome)
    {
        this.logger = logger;
        this.pythonHome = pythonHome;

        mokuroDir = tempDir.Directory("mokuro");
        ocrDir = tempDir.Directory("_ocr/mokuro");

        mokuroDir.Create();
        ocrDir.Create();
    }

    public ICorpusWorkExtractor.IContext Extract(Stream stream, IExtractProgress progress)
    {
        logger.LogInformation("Unzipping");
        using var zip = new ZipArchive(stream);
        zip.ExtractTo(mokuroDir, overwrite: true);

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

    private class ExtractorContextImpl : ICorpusWorkExtractor.IContext
    {
        public ExtractorContextImpl(IVirtualFs ocrDir, IVirtualFs mokuroDir, ILogger logger, IPersistentDirectory<PythonHome> pythonHome, IExtractProgress progress)
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

        public IEnumerable<ICorpusContent> GetExtraContent()
        {
            yield return evidenceCollection;
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

                var imagePath = FindImagePathForJson(path.GetFilenameWithoutExtension());
                if (imagePath == null)
                    continue; // TODO: warn for this? is it even possible?

                using var stream = mokuroDir.File(imagePath).Open(FileMode.Open);
                using var bitmap = SKBitmap.Decode(stream);

                foreach (var (block, blockNum) in json.Blocks.Select((x, i) => (x, i)))
                {
                    var scopedId = $"{page}-{blockNum}";
                    evidenceCollection.AddImage(scopedId, CutImage(bitmap, block.Box));
                    yield return new CorpusEntry(scopedId, string.Join("", block.Lines));
                }

                evidenceCollection.AddPageData(path.GetUnqualifiedFilePath(), json);
                progress.ReportProgress(page);
            }
        }

        private static SKImage CutImage(SKBitmap bitmap, MokuroBoundingBox box)
        {
            using var pixmap = new SKPixmap(bitmap.Info, bitmap.GetPixels());
            var subset = pixmap.ExtractSubset(new(
                (int)box.TopLeft.X,
                (int)box.TopLeft.Y,
                (int)box.BottomRight.X,
                (int)box.BottomRight.Y
            ));

            using var data = subset.Encode(SKJpegEncoderOptions.Default);
            return SKImage.FromEncodedData(data);
        }

        private FilePath FindImagePathForJson(string filename)
        {
            return mokuroDir.EnumerateFiles("*", SearchOption.AllDirectories)
                .Where(x => imageExtensions.Contains(x.GetExtension()))
                .FirstOrDefault(x => x.GetFilenameWithoutExtension() == filename);
        }

        private readonly IVirtualFs ocrDir, mokuroDir;
        private readonly ILogger logger;
        private readonly IPersistentDirectory<PythonHome> pythonHome;
        private readonly IExtractProgress progress;
        private readonly EvidenceCollection evidenceCollection;
        private static readonly HashSet<string> imageExtensions = new(StringComparer.InvariantCultureIgnoreCase) { "jpg", "png", "jpeg" };
    }

    private readonly ILogger logger;
    private readonly IPersistentDirectory<PythonHome> pythonHome;
    private readonly IVirtualFs mokuroDir;
    private readonly IVirtualFs ocrDir;

    public class PythonHome { private PythonHome() { } }
}
