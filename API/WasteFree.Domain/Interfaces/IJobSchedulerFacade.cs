namespace WasteFree.Domain.Interfaces;

public interface IJobSchedulerFacade
{
    Task ScheduleOneTimeJobAsync<TRequest>(
        string functionName,
        TRequest request,
        string description,
        CancellationToken cancellationToken = default,
        DateTime? executionTime = null,
        int retries = 3,
        int[]? retryIntervals = null);
}