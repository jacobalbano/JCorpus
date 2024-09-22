using Common.Addins.Extract;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Placeholders;
using GenHTTP.Modules.Reflection;
using JCorpus.Web.Converters;
using JCorpus.Web.Resources;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace JCorpus.Web.Transit;

record class TransitJob(
    Guid Id,

    string Description,

    [property: JsonConverter(typeof(InstantToJsDate))]
    Instant StartInstant,

    [property: JsonConverter(typeof(InstantToJsDate))]
    Instant? CompleteInstant,

    JobStatus Status,

    string Message
);

record class TransitJobResult<T>(
    Guid Id,
    string Description,
    Instant StartInstant,
    Instant? CompleteInstant,
    JobStatus Status,
    string Message,
    T Result
) : TransitJob(Id, Description, StartInstant, CompleteInstant, Status, Message);

static class LongRunningJobExtension
{
    public static TransitJob ToTransit(this ILongRunningJob job)
        => new(job.Id, job.Description, job.StartInstant, job.CompleteInstant, job.Status, job.Message);

    public static TransitJobResult<T> ToTransitResult<T>(this ILongRunningJob job, T result)
        => new(job.Id, job.Description, job.StartInstant, job.CompleteInstant, job.Status, job.Message, result);
}