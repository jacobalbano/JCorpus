using Common.Content;
using Common.DI;
using Common.IO;
using MeCab;
using MeCab.Extension.UniDic;
using Utility.Validators;
using Utility.Validators.Decorators;
using Utility;
using NodaTime;
using Common;
using Common.Addins.Analyze;
using Microsoft.Extensions.Logging;

namespace JCorpus.Jobs;

[AutoDiscover(AutoDiscoverOptions.Scoped)]
internal class VocabFrequency : IAnalysis
{
    public string Filename => $"VocabFrequency_{clock.GetTimestampFilename()}.psv";

    public Stream Run(IEnumerable<CorpusWork> works)
    {
        var results = new Dictionary<string, VocabStat>();
        var tagger = MeCabTagger.Create();

        int processed = 0;
        foreach (var work in works)
        {
            ++processed;
            logger.LogInformation("Processing {id}; {words} words in {works} works", work.UniqueId, results.Count, processed);
            foreach (var entry in corpus.GetWorkContent(work.UniqueId))
            {
                var analyzed = tagger.ParseToNodes(entry.Content)
                    .Where(x => x.CharType > 0)
                    .Where(x => rules.Validate(x.Feature.Split(",")))
                    .ToList();

                foreach (var node in analyzed)
                {
                    var baseForm = node.GetLForm();
                    if (!results.TryGetValue(baseForm, out var stat))
                        results[baseForm] = stat = new();

                    stat.Count++;
                    stat.AppearsIn.Add(work.UniqueId);
                    stat.MeCabFeatures.Add(node.Feature);
                    stat.SeenAs.Add(node.Surface);
                }
            }
        }

        var lines = results.OrderByDescending(x => GetScore(x.Value, processed))
            .Select(x => string.Join("|", GetScore(x.Value, processed), x.Key, string.Join("|", x.Value.MeCabFeatures)))
            .ToList();

        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(string.Join("\r\n", lines));
        writer.Flush();
        stream.Position = 0;
        return stream;
    }

    private static float GetScore(VocabStat stat, int works)
    {
        var coverage = stat.AppearsIn.Count / (float)works;
        return stat.Count * coverage;
    }

    public VocabFrequency(ICorpus corpus, ISystemClock clock, ILogger<VocabFrequency> logger)
    {
        this.corpus = corpus;
        this.clock = clock;
        this.logger = logger;
    }

    private class VocabStat
    {
        public int Count { get; set; }
        public HashSet<CorpusWorkId> AppearsIn { get; } = new();
        public HashSet<string> MeCabFeatures { get; } = new();
        public HashSet<string> SeenAs { get; } = new();

        public override string ToString() => $"{Count} hits in {AppearsIn.Count} works";
    }
    
    private readonly ICorpus corpus;
    private readonly ISystemClock clock;
    private readonly ILogger logger;
    private static readonly IValidator<string[]> rules = new AllValidator<string[]>(
        new AnyValidator<string[]>(
            new ExactFeatureMatchValidator(null, null, null, null, null, null, "*"),
            new ExactFeatureMatchValidator("動詞", "接尾"),
            new ExactFeatureMatchValidator("接頭詞", "名詞接続"),
            new ExactFeatureMatchValidator(null, "非自立"),
            new ExactFeatureMatchValidator(null, "固有名詞"),
            new ExactFeatureMatchValidator(null, "数"),
            new ExactFeatureMatchValidator("記号")
        ).Inverted(),

        new AnyValidator<string[]>(
            new ExactFeatureMatchValidator("名詞"),       // noun
            new ExactFeatureMatchValidator("動詞"),       // verb
            new ExactFeatureMatchValidator("形容動詞"),   // i-adj  
            new ExactFeatureMatchValidator("形容詞"),     // na-adj
            new ExactFeatureMatchValidator("副詞"),       // adverb
            new ExactFeatureMatchValidator("接頭詞"),     // prefix
            new ExactFeatureMatchValidator("連体詞"),     // prenom
            new ExactFeatureMatchValidator("助動詞)"      // aux verb
        )
    ));

    private record class ExactFeatureMatchValidator(params string[] Parts) : IValidator<string[]>
    {
        public bool Validate(string[] input)
        {
            for (int i = 0; i < Parts.Length; i++)
            {
                if (Parts[i] == null) continue;
                if (Parts[i] == input[i]) return true;
            }

            return false;
        }
    }
}
