using Common;
using Common.Content;
using Common.IO;
using Common.IO.Zip;
using MokuroWrapper;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace Mokuro;

/// <summary>
/// Represents a collection of image regions which the Mokuro OCR process identified
/// </summary>
public class EvidenceCollection : ICorpusContent, IDisposable
{
    public string ContentFileName => "MokuroRegions.zip";

    public IEnumerable<KeyValuePair<CorpusEntryId, SKImage>> Images => entryImages;
    public IEnumerable<KeyValuePair<string, MokuroPageData>> PageData => pageData;

    internal void AddImage(CorpusEntryId idForImage, SKImage image) => entryImages.Add(idForImage, image);
    internal void AddPageData(string filename, MokuroPageData page) => pageData.Add(filename, page);
    
    public bool TryGetImage(CorpusEntryId corpusEntryId, out SKImage image) => entryImages.TryGetValue(corpusEntryId, out image);
    public bool TryGetPageData(string filename, out MokuroPageData pageData) => this.pageData.TryGetValue(filename, out pageData);

    internal void ReadFromDirectory(IVirtualFs fs)
    {
        IFreezable.ThrowIfFrozen(this);
        foreach (var json in fs.EnumerateFiles("*.json"))
            AddPageData(json, JsonSerializer.Deserialize<MokuroPageData>(fs.File(json).ReadAllText()));

        foreach (var jpg in fs.EnumerateFiles("*.jpg"))
        {
            using var stream = fs.File(jpg).Open(FileMode.Open);
            AddImage(jpg.GetFilenameWithoutExtension(), SKImage.FromEncodedData(stream));
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        foreach (var (_, img) in Images)
            img.Dispose();
    }

    #region ICorpusContent
    void ICorpusContent.Read(IVirtualFile inFile)
    {
        using var zipStream = inFile.Open(FileMode.Open);
        using var archive = new ZipArchive(zipStream);
        ReadFromDirectory(new ZipFilesystem(archive));
    }

    void ICorpusContent.Write(IVirtualFile outFile)
    {
        using var zipStream = outFile.Open(FileMode.OpenOrCreate);
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Update);
        var fs = new ZipFilesystem(archive);

        foreach (var (filename, image) in Images)
        {
            using var fileOut = fs.File($"{filename}.jpg").Open(FileMode.CreateNew);
            using var imgStream = image.EncodedData.AsStream();
            imgStream.CopyTo(fileOut);
        }

        foreach (var (filename, data) in PageData)
            fs.File(filename).WriteAllText(JsonSerializer.Serialize(data, options));
    }
    #endregion

    #region IFreezable
    bool IFreezable.Frozen => frozen;
    void IFreezable.Freeze() => frozen = true;
    private bool frozen;
    #endregion

    private readonly Dictionary<CorpusEntryId, SKImage> entryImages = new();
    private readonly Dictionary<string, MokuroPageData> pageData = new();
    private readonly JsonSerializerOptions options = new() { Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) };
}
