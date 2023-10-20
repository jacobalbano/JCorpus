using Common;
using Common.DI;
using Common.IO;
using JCorpus.Implementation.IO.Filesystem;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Utility;

namespace JCorpus;

record class ConfigurableStage<T>(
    string TypeName,
    JsonDocument? ConfigurationJson
);

record class JobParams(
    ConfigurableStage<ICorpusWorkEnumerator> Enumerator,
    ConfigurableStage<ICorpusWorkExtractor> Extractor
);