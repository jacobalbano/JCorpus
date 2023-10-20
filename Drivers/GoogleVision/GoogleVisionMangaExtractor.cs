using Common;
using Common.Content;
using Common.Utility;
using Google.Cloud.Vision.V1;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data.SqlTypes;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GoogleVision;

public class GoogleVisionMangaExtractor : ICorpusWorkExtractor
{
    public ICorpusWorkExtractorContext Extract(CorpusWork work, IExtractProgress progress)
    {
        return new ExtractorContextImpl(work);
    }

    private class ExtractorContextImpl : ICorpusWorkExtractorContext
    {
        private readonly double confidenceThreshold = 0.4;

        public ExtractorContextImpl(CorpusWork work)
        {
            this.work = work;
        }

        public void Dispose()
        {
        }

        public async IAsyncEnumerable<CorpusEntry> EnumerateEntries([EnumeratorCancellation] CancellationToken ct)
        {
            using var zip = ZipFile.OpenRead(work.Uri);
            foreach (var entry in zip.Entries.Where(x => isImageFile.IsMatch(x.Name)))
            {
                using var stream = entry.Open();
                var result = await ApiCache.Instance.DetectText(work.UniqueId, entry.Name, stream);

                var furigana = new List<CorpusFurigana>();
                int blockNum = 0;
                foreach (var block in result.TextBlocks)
                {
                    var scopedUniqueId = $"{entry.Name}/{blockNum++}";
                    //var traits = AnalyzeTextBlock(block);
                    if (block.Score <= confidenceThreshold)
                        continue;

                    var (rubyChars, chars) = block.Paragraphs
                        .SelectMany(x => x.Words)
                        .SelectMany(x => x.Symbols)
                        .SplitBy(
                            x => false, //traits.RubyGlyphSize.IsInRange(x.BoundingBox.Height),
                            x => true
                        );

                    FindLines(block);
                    //var sb = new StringBuilder();

                    //int rubyStart = -1;
                    //var rubyBuffer = new List<string>();
                    //foreach (var c in chars)
                    //{
                    //    if (lookup.TryGetValue(c, out var belongsTo))
                    //    {
                    //        // might be ruby, so don't add it to the final result
                    //        if (rubyStart < 0)
                    //            rubyStart = sb.Length;

                    //        rubyBuffer.Add(c.Text);
                    //    }
                    //    else
                    //    {
                    //        sb.Append(c.Text);
                    //        flushRuby(furigana, scopedUniqueId, sb, ref rubyStart, rubyBuffer);
                    //    }
                    //}

                    //flushRuby(furigana, scopedUniqueId, sb, ref rubyStart, rubyBuffer);
                    yield return new(scopedUniqueId, string.Join("", chars.Select(x => x.Text)));
                }
            }

            static void flushRuby(IList<CorpusFurigana> furigana, string scopedUniqueId, StringBuilder sb, ref int rubyStart, List<string> rubyBuffer)
            {
                if (rubyStart < 0)
                {
                    furigana.Add(new(string.Join("", rubyBuffer), scopedUniqueId, rubyStart, sb.Length));
                    rubyBuffer.Clear();
                    rubyStart = -1;
                }
            }
        }

        private static void FindLines(TextBlock block)
        {
            int countHorizontal = 0, countVertical = 0;
            foreach (var p in block.Paragraphs)
            {
                if (p.BoundingBox.Width > p.BoundingBox.Height)
                    countHorizontal++;
                else if (p.BoundingBox.Height > p.BoundingBox.Width)
                    countVertical++;
            }

            var orientation = Orientation.Unknown;
            if (countHorizontal > countVertical)
                orientation = Orientation.Horizontal;
            else if (countHorizontal < countVertical)
                orientation = Orientation.Vertical;

            var allGlyphs = block.Paragraphs
                .SelectMany(x => x.Words)
                .SelectMany(x => x.Symbols)
                .ToList();

            var centers = allGlyphs
                .Select(x => x.BoundingBox)
                .Select(x => orientation switch
                {
                    Orientation.Vertical => x.BoundsTopLeft.X + (x.Width / 2),
                    Orientation.Horizontal => x.BoundsTopLeft.Y + (x.Height / 2),
                    _ => throw new Exception("Unknown orientation"),
                })
                .ToArray();

            var clustered = GetClusters(centers);
            

        }
        /*
        private static Dictionary<Symbol, Symbol> MatchRubyToText(IReadOnlyList<Symbol> allChars, TextBlockTraits traits)
        {
            var result = new Dictionary<Symbol, Symbol>();

            if (!rubyChars.Any())
                return result;

            var charColumnClusters = GetCenterClusters(chars);
            var rubyColumnClusters = GetCenterClusters(rubyChars);
            var bestColumnForRuby = rubyChars.ToDictionary(x => x, x => charColumnClusters
                .Select(x => x.Average())
                .Distinct()
                .OrderBy(y => x.BoundingBox.BoundsTopLeft.X - y)
                .First());

            foreach (var rb in rubyChars)
            {
                if (!bestColumnForRuby.TryGetValue(rb, out var column))
                    throw new Exception("No char column cluster");

                var candidates = chars
                    .Where(x => x.BoundingBox.BoundsTopLeft.X < column && column < x.BoundingBox.BoundsBottomRight.X)
                    .Where(x => rb.BoundingBox.BoundsTopLeft.X > x.BoundingBox.BoundsBottomRight.X)
                    .Where(x => rb.BoundingBox.BoundsTopLeft.Y >= x.BoundingBox.BoundsTopLeft.Y)
                    .Where(x => rb.BoundingBox.BoundsBottomRight.Y <= x.BoundingBox.BoundsBottomRight.Y)
                    .ToList();

                if (candidates.Count == 1)
                    result[rb] = candidates.Single();
            }

            return result;

            static IReadOnlyList<int>[] GetCenterClusters(List<Symbol> rubyChars)
            {
                return GetClusters(rubyChars
                    .Select(x => x.BoundingBox)
                    .Select(x => x.BoundsTopLeft.X + (x.Width / 2))
                    .ToList()
                );
            }
        }
        */
        private TextBlockTraits AnalyzeTextBlock(TextBlock block)
        {
            var allGlyphs = block.Paragraphs
                .SelectMany(x => x.Words)
                .SelectMany(x => x.Symbols)
                .Select(x => x.BoundingBox.Height)
                .OrderBy(x => x)
                .ToList();

            var clusters = GetClusters(allGlyphs);
            ClusterRange glyphSize = new(), rubySize = new();

            // standard 
            if (clusters.Length >= 1)
            {
                var cluster = clusters.Last();
                glyphSize = new(cluster.Min(), cluster.Max());
            }

            // if we have furigana
            if (clusters.Length >= 2)
            {
                var cluster = clusters.First();
                rubySize = new(cluster.Min(), cluster.Max());
            }

            // if there's an unhandled case
            if (clusters.Length > 2)
                throw new Exception($"Unexpected cluster count {clusters.Length}; investigate this!");

            int countHorizontal = 0, countVertical = 0;
            foreach (var p in block.Paragraphs)
            {
                if (p.BoundingBox.Width > p.BoundingBox.Height)
                    countHorizontal++;
                else if (p.BoundingBox.Height > p.BoundingBox.Width)
                    countVertical++;
            }

            var orientation = Orientation.Unknown;
            if (countHorizontal > countVertical)
                orientation = Orientation.Horizontal;
            else if (countHorizontal < countVertical)
                orientation = Orientation.Vertical;

            return new(orientation, glyphSize, rubySize);
        }

        private static IReadOnlyList<int>[] GetClusters(IReadOnlyList<int> input)
        {
            var gaps = input
                .Skip(1)
                .Select((height, lag) => height - input[lag])
                .OrderBy(x => x)
                .Where(x => x > 0)
                .DefaultIfEmpty(0)
                .ToList();

            return OrderedCluster(input, maxDiff: gaps.Max() - gaps.Min() + 1)
                .ToArray();
            
            static IEnumerable<IReadOnlyList<int>> OrderedCluster(IReadOnlyList<int> data, double maxDiff)
            {
                var currentGroup = new List<int>();
                foreach (var item in data)
                {
                    var testGroup = new List<int>(currentGroup) { item };
                    var mean = testGroup.Average();
                    if (testGroup.All(x => Math.Abs(mean - x) < maxDiff))
                        currentGroup = testGroup;
                    else
                    {
                        yield return currentGroup;
                        currentGroup = new() { item };
                    }
                }

                if (currentGroup.Any())
                    yield return currentGroup;
            }
        }

        private CorpusWork work;
    }

    private static readonly Regex isImageFile = new(@"\.(png|jpg|jpeg)$", RegexOptions.Compiled);
    private readonly ILogger<GoogleVisionMangaExtractor> logger;

    public GoogleVisionMangaExtractor(ILogger<GoogleVisionMangaExtractor> logger)
    {
        this.logger = logger;
    }

    private record class TextBlockTraits(
        Orientation Orientation,
        ClusterRange StandardGlyphSize,
        ClusterRange RubyGlyphSize
    );

    private record class ClusterRange(double Min = 0, double Max = 0)
    {
        public bool IsInRange(double value) => value >= Min && value <= Max;
    };

    private enum Orientation { Unknown, Horizontal, Vertical }
}

internal static class Ext
{
    public static IReadOnlyList<List<T>> SplitBy<T>(this IEnumerable<T> self, params Func<T, bool>[] selectors)
    {
        var result = new List<T>[selectors.Length];
        for (int i = 0; i < selectors.Length; i++)
            result[i] = new List<T>();

        foreach (var item in self)
        {
            for (int i = 0; i < selectors.Length; i++)
            {
                if (selectors[i](item))
                {
                    result[i].Add(item);
                    break;
                }
            }
        }

        return result;
    }

    public static void Deconstruct<T>(this IReadOnlyList<T> list, out T p1, out T p2) =>  (p1, p2) = (list[0], list[1]);
}