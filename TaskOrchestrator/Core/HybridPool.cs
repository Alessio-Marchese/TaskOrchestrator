using TaskOrchestrator.Utils;

namespace TaskOrchestrator.Core;

public class HybridPool : IDisposable
{
    public AsyncPool AsyncPoolService { get; private set; }
    public SyncPool SyncPoolService { get; private set; }

    private HybridPool(AsyncPool asyncPoolService, SyncPool syncPoolService)
    {
        AsyncPoolService = asyncPoolService;
        SyncPoolService = syncPoolService;
    }

    public static HybridPool Create(Options options)
    {
        return new HybridPool(new AsyncPool(options), new SyncPool(options));
    }

    public static HybridPool Create(Options asyncPoolOptions, Options syncPoolOptions)
        => new HybridPool(new AsyncPool(asyncPoolOptions), new SyncPool(syncPoolOptions));

    public void Enqueue(Func<Task> task, int weight = 0)
    {
        if (AsyncPoolService.Options.Workers < 1 && AsyncPoolService.Options.MaxElasticWorkers < 1)
            throw new InvalidOperationException("Must have at least 1 worker in the configuration");

        AsyncPoolService.Enqueue(task, weight);
    }

    public void Enqueue(Action action, int weight = 0)
    {
        if (SyncPoolService.Options.Workers < 1 && SyncPoolService.Options.MaxElasticWorkers < 1)
            throw new InvalidOperationException("Must have at least 1 worker in the configuration");

        SyncPoolService.Enqueue(action, weight);
    }

    public int PendingWorkCount()
        => AsyncPoolService.PendingWorkCount() + SyncPoolService.PendingWorkCount();

    public int PendingAsyncWorkCount()
        => AsyncPoolService.PendingWorkCount();

    public int PendingSyncWorkCount()
        => SyncPoolService.PendingWorkCount();

    public int CurrentElasticWorkers()
        => AsyncPoolService.CurrentElasticWorkersCount() + SyncPoolService.CurrentElasticWorkersCount();

    public int CurrentAsyncElasticWorkers()
        => AsyncPoolService.CurrentElasticWorkersCount();

    public int CurrentSyncElasticWorkers()
        => SyncPoolService.CurrentElasticWorkersCount();

    public void Dispose()
    {
        SyncPoolService.Dispose();
        AsyncPoolService.Dispose();
    }
}
