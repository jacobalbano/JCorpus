using Common;
using Common.Content;
using Common.IO;
using Common.Addins.Analyze;
using Microsoft.Extensions.Logging;
using System.Text;
using Utility;
using Utility.IO;

namespace Frequency;

public class CharacterCounts : IAnalysis
{
    public string Filename => $"CharacterFrequency_{clock.GetTimestampFilename()}.txt";

    public Stream Run(IEnumerable<CorpusWork> works)
    {
        var results = new Dictionary<string, ulong>();

        ulong processed = 0;
        foreach (var work in works)
        {
            logger.LogInformation("Processing {id}", work.UniqueId);

            if (!corpus.TryGetWorkExtraContent(work.UniqueId, out CharacterFrequency freq))
            {
                freq.ReadCorpusContent(corpus.GetWorkContent(work.UniqueId));
                logger.LogWarning("Accelerate {workId} with {accelerator} for improved performance in the future", work.UniqueId, nameof(CharacterAccelerator));
            }

            foreach (var (character, count) in freq)
                results[character] = count + (results.TryGetValue(character, out var total) ? total : 0);

            ++processed;
            logger.LogInformation("Done; {chars} unique characters in {works} works so far", results.Count, processed);
        }

        var str = string.Join("\r\n", results.OrderByDescending(x => x.Value)
            .Select(x => string.Join("|", x.Key, x.Value)));

        return new MemoryStream(Encoding.UTF8.GetBytes(str));
    }

    public CharacterCounts(ICorpus corpus, ILogger<CharacterCounts> logger, ISystemClock clock)
    {
        this.corpus = corpus;
        this.logger = logger;
        this.clock = clock;
    }

    private readonly ICorpus corpus;
    private readonly ILogger logger;
    private readonly ISystemClock clock;
}
