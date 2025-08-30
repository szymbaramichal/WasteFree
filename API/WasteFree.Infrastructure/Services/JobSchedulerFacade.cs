using TickerQ.Utilities;
using TickerQ.Utilities.Interfaces.Managers;
using TickerQ.Utilities.Models.Ticker;
using WasteFree.Shared.Interfaces;

namespace WasteFree.Infrastructure.Services;

public class JobSchedulerFacade(ITimeTickerManager<TimeTicker> timeTickerManager) : IJobSchedulerFacade
{
    public async Task ScheduleOneTimeJobAsync<TRequest>(
        string functionName,
        TRequest request,
        string description,
        CancellationToken cancellationToken = default,
        DateTime? executionTime = null,
        int retries = 3,
        int[]? retryIntervals = null)
    {
        var ticker = new TimeTicker
        {
            Request = TickerHelper.CreateTickerRequest(request),
            ExecutionTime = executionTime ?? DateTime.UtcNow,
            Function = functionName,
            Description = description,
            Retries = retries,
            RetryIntervals = retryIntervals ?? [1, 5, 15]
        };
        await timeTickerManager.AddAsync(ticker, cancellationToken);
    }
}
