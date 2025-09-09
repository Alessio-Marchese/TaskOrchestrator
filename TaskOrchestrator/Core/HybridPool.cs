using TaskOrchestrator.Utils;

namespace TaskOrchestrator.Core;

public class HybridPool : IDisposable
{
    private readonly AsyncPool _asyncPoolService;
    private readonly SyncPool _syncPoolService;

    private HybridPool(AsyncPool asyncPoolService, SyncPool syncPoolService)
    {
        _asyncPoolService = asyncPoolService;
        _syncPoolService = syncPoolService;
    }

    public static HybridPool Create(Options options)
    {
        return new HybridPool(new AsyncPool(options), new SyncPool(options));
    }

    public static HybridPool Create(Options asyncPoolOptions, Options syncPoolOptions)
        => new HybridPool(new AsyncPool(asyncPoolOptions), new SyncPool(syncPoolOptions));

    public void Enqueue(Func<Task> task, int weight = 0)
    {
        if (_asyncPoolService.Options.Workers < 1 && _asyncPoolService.Options.MaxElasticWorkers < 1)
            throw new InvalidOperationException("Must have at least 1 worker in the configuration");

        _asyncPoolService.Enqueue(task, weight);
    }

    public void Enqueue(Action action, int weight = 0)
    {
        if (_syncPoolService.Options.Workers < 1 && _syncPoolService.Options.MaxElasticWorkers < 1)
            throw new InvalidOperationException("Must have at least 1 worker in the configuration");

        _syncPoolService.Enqueue(action, weight);
    }

    public int PendingWorkCount()
        => _asyncPoolService.PendingWorkCount() + _syncPoolService.PendingWorkCount();

    public int PendingAsyncWorkCount()
        => _asyncPoolService.PendingWorkCount();

    public int PendingSyncWorkCount()
        => _syncPoolService.PendingWorkCount();

    public int CurrentElasticWorkers()
        => _asyncPoolService.CurrentElasticWorkersCount() + _syncPoolService.CurrentElasticWorkersCount();

    public int CurrentAsyncElasticWorkers()
        => _asyncPoolService.CurrentElasticWorkersCount();

    public int CurrentSyncElasticWorkers()
        => _syncPoolService.CurrentElasticWorkersCount();

    public void Dispose()
    {
        _syncPoolService.Dispose();
        _asyncPoolService.Dispose();
    }
}
