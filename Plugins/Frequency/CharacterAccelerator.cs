using Common;
using Common.Content;
using Common.Addins.Accelerate;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frequency;

public class CharacterAccelerator : IAccelerator
{
    public ICorpusContent ContentObject => freq;

    public void Accelerate(CorpusWork work, IEnumerable<CorpusEntry> content)
    {
        logger.LogInformation("Processing {work}", work.UniqueId);
        freq.ReadCorpusContent(content);
        logger.LogInformation("Done");
    }

    public CharacterAccelerator(ILogger<CharacterAccelerator> logger)
    {
        this.logger = logger;
    }

    private readonly CharacterFrequency freq = new();
    private readonly ILogger logger;
}
