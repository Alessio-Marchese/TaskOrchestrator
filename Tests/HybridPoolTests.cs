using TaskOrchestrator.Core;
using TaskOrchestrator.Utils;

namespace Tests;

public class HybridPoolTests
{
    [Fact]
    public void FirstVeryNoobTest()
    {
        List<string> logs = new();
        var tcs = new TaskCompletionSource<bool>();
        int tasksToExecute = 50;

        HybridPool? hybridPool = null;

        hybridPool = HybridPool.Create(Options.GetElasticOptions(2, 5, TimeSpan.FromSeconds(10), 
            (info) => logs.Add($"BeforeExecution | Function Id: {info.Id} | Function Weight: {info.Weight} | PendingWork: {hybridPool!.PendingWorkCount()} | TYPE: {(info.IsAsync ? "Async" : "Sync")} | ElasticWorkers: {hybridPool.CurrentElasticWorkers()}"),
            null,
            (info, ex) => logs.Add($"FAILED | Function Id: {info.Id}\nException: {ex}")));

        for (int i = tasksToExecute; i > 0; i--)
        {
            hybridPool.Enqueue(
                () =>
                {
                    Thread.Sleep(100);
                }, i);
        }

        for (int i = tasksToExecute; i > 0; i--)
        {
            hybridPool.Enqueue(
                async () =>
                {
                    await Task.Delay(100);
                }, i);
        }

        Thread.Sleep(10000);

        var log = string.Join("\n", logs);

        Assert.NotNull(log);
    }
}