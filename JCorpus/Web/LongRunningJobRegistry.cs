using Common.DI;
using JCorpus.DI;
using Microsoft.Extensions.Logging;
using NodaTime;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Utility;

namespace JCorpus.Web;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum JobStatus
{
    Pending = 0,
    Running = 1,
    Complete = 2,
    Cancelled = 3,
    Error = 4
}

/// <summary>
/// Represents a long-running job. 
/// </summary>
internal interface ILongRunningJob : IDisposable
{
    /// <summary>The ID of the job</summary>
    Guid Id { get; }

    /// <summary>A description of the job. Will be visible in the dashboard.</summary>
    string Description { get; }

    /// <summary>The cancellation token associated with this job</summary>
    CancellationToken Token { get; }

    /// <summary>The instant that this job was created</summary>
    Instant StartInstant { get; }

    /// <summary>The instant that this job was created</summary>
    Instant? CompleteInstant { get; }

    /// <summary>The job's status</summary>
    JobStatus Status { get; }

    /// <summary>If <see cref="Status"/> is <see cref="JobStatus.Error"/>, contains the exception string.</summary>
    string Message { get; }
}

[AutoDiscover(AutoDiscoverOptions.Transient)]
internal class LongRunningJobRegistry : IEnumerable<ILongRunningJob>
{
    public LongRunningJobRegistry(ISystemClock clock, ILogger<LongRunningJobRegistry> logger)
    {
        this.clock = clock;
        this.logger = logger;
    }

    public ILongRunningJob NewJob(string description)
    {
        var job = new JobImpl() { Description = description, StartInstant = clock.GetCurrentInstant() };
        jobs.TryAdd(job.Id, job);
        return job;
    }

    public void Cancel(Guid id)
    {
        if (jobs.TryGetValue(id, out var job) && job.Status <= JobStatus.Complete)
            job.Dispose();
    }

    public bool TryGet(Guid id, out ILongRunningJob job)
    {
        job = default;
        if (jobs.TryGetValue(id, out var result))
        {
            job = result;
            return true;
        }

        return false;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public IEnumerator<ILongRunningJob> GetEnumerator()
    {
        foreach (var item in jobs.Values)
            yield return item;
    }

    public ILongRunningJob RunWrapped(string description, Func<ILongRunningJob, Task> value)
    {
        var job = (JobImpl) NewJob(description);
        Task.Run(async () => {
            job.Status = JobStatus.Running;
            try
            {
                await value(job);
                job.Status = JobStatus.Complete;
                job.CompleteInstant = clock.GetCurrentInstant();
            }
            catch (TaskCanceledException)
            {
                job.Status = JobStatus.Cancelled;
            }
            catch (Exception e)
            {
                job.Message = e.ToString();
                job.Status = JobStatus.Error;
                logger.LogError(e, "Job threw an exception");
            }
        });

        return job;
    }

    public IReadOnlyList<Guid> Prune(Duration olderThan)
    {
        var now = clock.GetCurrentInstant();
        var prune = jobs.Values
            .Where(x => x.Status >= JobStatus.Complete)
            .Where(x => x.CompleteInstant < now - olderThan)
            .Select(x => x.Id)
            .ToList();

        return prune.Where(x => jobs.TryRemove(x, out _))
            .ToList();
    }

    private readonly ConcurrentDictionary<Guid, JobImpl> jobs = new();
    private readonly ISystemClock clock;
    private readonly ILogger logger;

    private sealed class JobImpl : ILongRunningJob
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Description { get; init; }
        public Instant StartInstant { get; init; }
        public Instant? CompleteInstant { get; set; } = null;
        public JobStatus Status { get; set; } = JobStatus.Pending;
        public string Message { get; set; } = string.Empty;

        public readonly CancellationTokenSource TokenSource = new();
        public CancellationToken Token => TokenSource.Token;

        public void Dispose()
        {
            if (disposed)
                return;

            TokenSource.Cancel();
            disposed = true;
        }

        private bool disposed = false;
    }
}
