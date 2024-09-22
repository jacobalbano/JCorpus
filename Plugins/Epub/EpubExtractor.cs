using Common;
using Common.Addins.Extract;
using Common.Content;
using Common.Content.Collections;
using Common.IO;
using Common.IO.Zip;
using Common.Utility;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Utility;

namespace Epub;

public class EpubExtractor : ICorpusWorkExtractor
{
    public IReadOnlyList<string> InTags => new[] { "Epub" };

    public EpubExtractor(ILogger<EpubExtractor> logger)
    {
        this.logger = logger;
    }

    public ICorpusWorkExtractor.IContext Extract(Stream stream, IExtractProgress progress)
    {
        return new Context(stream, logger);
    }

    private class Context : ICorpusWorkExtractor.IContext
    {
        public void Dispose()
        {
            zip.Dispose();
        }

        private record class Line(string Text, IReadOnlyList<CorpusFurigana> Furigana);

        private static readonly XNamespace containerNs = "urn:oasis:names:tc:opendocument:xmlns:container";
        private static readonly XNamespace opfNs = "http://www.idpf.org/2007/opf";

        public async IAsyncEnumerable<CorpusEntry> EnumerateEntries(CancellationToken ct)
        {
            var epub = new ZipFilesystem(zip);
            foreach (var (name, doc) in GetContentFiles(epub))
            {
                int number = 0;
                foreach (var node in GetSentenceNodes(doc))
                {
                    foreach (var (text, furigana) in BreakSentences(node))
                    {
                        var id = $"{name}-{number++}";
                        if (furigana.Any())
                            this.furigana.Add(id, furigana);
                        yield return new(id, text);
                    }
                }
            }
        }

        private IEnumerable<(string, IReadOnlyList<CorpusFurigana>)> BreakSentences(HtmlNode doc)
        {
            var breaker = new SentenceBreaker();
            var tokens = CreateTextTokens(doc).ToList();

            var sb = new StringBuilder();
            var furigana = new List<CorpusFurigana>();
            foreach (var token in tokens)
            {
                switch (token)
                {
                    case TextToken text:
                        sb.Append(text.Text);
                        if (breaker.Break(text.Text))
                            yield return reset();
                        break;
                    case RubyToken ruby:
                        furigana.Add(new(sb.Length, sb.Length + ruby.RubyText.Length, ruby.RtText));
                        sb.Append(ruby.RubyText);
                        break;
                    default:
                        throw new Exception($"Invalid token type {token.GetType()}");
                }
            }

            if (sb.Length > 0)
                yield return reset();

            (string, List<CorpusFurigana> furigana) reset()
            {
                var result = (sb.ToString(), furigana);
                breaker.Reset();
                sb.Clear();
                furigana = new(0);
                return result;
            }
        }

        private IEnumerable<Token> CreateTextTokens(HtmlNode doc)
        {
            foreach (var node in doc.Descendants())
            {
                switch (node.NodeType)
                {
                    case HtmlNodeType.Element:
                        if (node.Name == "ruby")
                            yield return ParseRuby(node.Descendants());
                        break;
                    case HtmlNodeType.Text:
                        if (node.ParentNode.Name == "ruby" || node.ParentNode.Name == "rt")
                            continue;
                        var text = node.InnerText.Trim();
                        foreach (var part in SentenceBreaker.BreakInitial(text))
                            yield return new TextToken(part);
                        break;
                    case HtmlNodeType.Comment:
                        continue;
                    default:
                        logger.LogDebug("Unhandled node type {type}", node.NodeType);
                        break;
                }
            }
        }

        private RubyToken ParseRuby(IEnumerable<HtmlNode> nodes)
        {
            string text = "", rtText = "";
            foreach (var child in nodes)
            {
                switch (child.NodeType)
                {
                    case HtmlNodeType.Text:
                        var parentName = child.ParentNode.Name;
                        if (parentName == "ruby")
                            text += child.InnerText;
                        else if (parentName == "rt")
                            rtText += child.InnerText;
                        else logger.LogDebug("#text node with parent {name} found inside ruby tag", parentName);
                        break;
                    case HtmlNodeType.Element:
                    case HtmlNodeType.Comment:
                        continue;
                    default:
                        logger.LogDebug("Unhandled node type {type}", child.NodeType);
                        break;
                }
            }

            return new(text.Trim(), rtText.Trim());
        }

        private record class RubyToken(string RubyText, string RtText) : Token();
        private record class TextToken(string Text) : Token();
        private record class Token();

        private static IEnumerable<(string, HtmlDocument)> GetContentFiles(IVirtualFs epub)
        {
            var containerFile = epub.File("META-INF/container.xml");
            if (!containerFile.Exists) throw new Exception("Malformed epub: no META-INF/container.xml");
            var content = XElement.Parse(containerFile.ReadAllText());

            foreach (var rootfile in content.Element(containerNs + "rootfiles").Elements())
            {
                var path = rootfile.Attribute("full-path")?.Value;
                var opfFile = epub.File(path);
                if (!opfFile.Exists) throw new Exception($"Missing rootfile at {path}");

                var opf = XElement.Parse(opfFile.ReadAllText());
                var manifest = opf.Element(opfNs + "manifest");
                var basePath = opfFile.ContainingDirectory;
                foreach (var item in manifest.Elements())
                {
                    if (item.Attribute("media-type")?.Value != "application/xhtml+xml") continue;
                    var itemFile = basePath.File(item.Attribute("href")?.Value ?? throw new Exception("No href attribute"));
                    if (!itemFile.Exists) throw new Exception($"Missing file {itemFile.GetFullyQualifiedPath()}");

                    var doc = new HtmlDocument();
                    doc.LoadHtml(itemFile.ReadAllText());
                    yield return (itemFile.Filename.GetFilenameWithoutExtension(), doc);
                }
            }
        }

        private static IEnumerable<HtmlNode> GetSentenceNodes(HtmlDocument doc)
        {
            var sentenceDivs = doc.DocumentNode
                .Descendants("div")
                .Where(x => !x.Descendants("div").Any())
                .ToList();

            foreach (var div in sentenceDivs)
            {
                var paragraphs = div.Descendants("p")
                    .Where(x => !string.IsNullOrEmpty(x.InnerText?.Trim()))
                    .ToList();

                if (paragraphs.Any())
                {
                    foreach (var p in paragraphs)
                        yield return p;
                }
                else
                {
                    yield return div;
                }
            }
        }

        public IEnumerable<ICorpusContent> GetExtraContent()
        {
            yield return furigana;
        }

        private readonly FuriganaCollection furigana = new();
        private readonly ZipArchive zip;
        private readonly ILogger logger;

        public Context(Stream stream, ILogger logger)
        {
            zip = new ZipArchive(stream);
            this.logger = logger;
        }
    }

    private readonly ILogger logger;
}