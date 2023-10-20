namespace Common.Content;

/// <summary>
/// A collection of corpus text. May represent one volume of manga or novel, one chapter of a webnovel, etc.
/// </summary>
/// <param name="UniqueId">An arbitrary id that must uniquely identify this work in the corpus.</param>
/// <param name="Uri">A URI representing the source of the work. May be an URL, a file path, etc.</param>
public record class CorpusWork(
    UniqueId UniqueId,
    string Uri
);