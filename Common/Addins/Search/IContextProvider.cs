using Common.Content;
using Common.DI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.Addins.Search;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ContextKind
{
    None,
    Image,
    Text,
    Url,
}

/// <summary>
/// 
/// </summary>
/// <param name="Kind">The kind of evidence, so the frontend can know how to interpret it.</param>
/// <param name="Data"></param>
public record class CorpusEntryContext(
    ContextKind Kind,
    byte[] Data
);

/// <summary>Defines a means by which <see cref="CorpusEntryContext"/> can be retrieved for a given <see cref="CorpusWork"/></summary>
[AutoDiscover(AutoDiscoverOptions.Implementations | AutoDiscoverOptions.Transient)]
public interface IContextProvider
{
    /// <summary>Returns evidence from the given corpus entry, if any exists.</summary>
    /// <param name="corpusWorkId">The ID of thw work to be queriesd.</param>
    /// <param name="corpusEntryId">The ID of the entry to get evidence for.</param>
    IEnumerable<CorpusEntryContext> GetEvidence(CorpusWorkId corpusWorkId, CorpusEntryId corpusEntryId);
}
