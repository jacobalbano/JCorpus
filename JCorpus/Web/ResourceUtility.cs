using Common.Addins;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Reflection;
using JCorpus.DI;
using JCorpus.Web.Transit;
using JCorpus.Web.Transit.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCorpus.Web;

internal static class ResourceUtility
{
    public static void CancelJob(this LongRunningJobRegistry jobs, Guid jobId)
    {
        if (!jobs.TryGet(jobId, out _))
            throw new ProviderException(ResponseStatus.NotFound, "Not found");

        jobs.Cancel(jobId);
    }

    /// <summary>
    /// Get an object which will communicate the pending nature of an HTTP result to the caller.<br />
    /// </summary>
    /// <typeparam name="T">The result type. Should be inferrable by <paramref name="getValue"/></typeparam>
    /// <param name="jobs">The <see cref="LongRunningJobRegistry"/> to refer to.</param>
    /// <param name="jobId">The Id of the <see cref="ILongRunningJob"/> to pull out. If the job doesn't exist in the registry, a 404 exception is thrown.</param>
    /// <param name="getValue">The value to return in the case that the job in question is <see cref="JobStatus.Complete"/>. If this function returns false, a 410 exception is thrown.</param>
    /// <exception cref="ProviderException">Returns 404 (not found) or 410 (gone) to the caller. Do not handle this exception.</exception>
    public static Result<TransitJobResult<T>> GetPendingResult<T>(this LongRunningJobRegistry jobs, Guid jobId, Func<(bool, T)> getValue)
    {
        if (!jobs.TryGet(jobId, out var job))
            throw new ProviderException(ResponseStatus.NotFound, "Not found");

        T results = default;
        if (job.Status == JobStatus.Error)
            return new Result<TransitJobResult<T>>(job.ToTransitResult(results))
                .Status(ResponseStatus.NotFound);

        if (job.Status != JobStatus.Complete)
            return new Result<TransitJobResult<T>>(job.ToTransitResult(results))
                .Status(ResponseStatus.Accepted);

        (bool found, results) = getValue();
        if (!found)
            throw new ProviderException(ResponseStatus.Gone, "Gone");

        return new(job.ToTransitResult(results));
    }

    /// <summary>
    /// Get an object which will communicate the pending nature of an HTTP result to the caller.<br />
    /// </summary>
    /// <param name="jobs">The <see cref="LongRunningJobRegistry"/> to refer to.</param>
    /// <param name="jobId">The Id of the <see cref="ILongRunningJob"/> to pull out. If the job doesn't exist in the registry, a 404 exception is thrown.</param>
    /// <exception cref="ProviderException">Returns 404 (not found) to the caller. Do not handle this exception.</exception>
    public static Result<TransitJob> GetPendingResult(this LongRunningJobRegistry jobs, Guid jobId)
    {
        if (!jobs.TryGet(jobId, out var job))
            throw new ProviderException(ResponseStatus.NotFound, "Not found");

        if (job.Status != JobStatus.Complete)
            return new Result<TransitJob>(job.ToTransit())
                .Status(ResponseStatus.Accepted);

        return new Result<TransitJob>(job.ToTransit());
    }
}
