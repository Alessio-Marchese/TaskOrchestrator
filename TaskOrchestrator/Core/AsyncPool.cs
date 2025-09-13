using System.Collections.Concurrent;
using TaskOrchestrator.Internal;
using TaskOrchestrator.Utils;

namespace TaskOrchestrator.Core;

internal class AsyncPool : IDisposable
{
    public readonly Options Options;
    public event Action<TaskInfo>? BeforeExecution;
    public event Action<TaskInfo>? AfterExecution;
    public event Action<TaskInfo, Exception>? OnFailure;

    private readonly CancellationTokenSource _cts = new();
    private readonly List<Task> _workers = new();
    private readonly ConcurrentBag<Task> _elasticWorkers = new();
    private volatile int _elasticWorkersCount = 0;
    private readonly SemaphoreSlim _signal;
    private readonly AsyncQueue _queue = new();

    public AsyncPool(Options options)
    {
        Options = options;
        _signal = new(Options.Workers);
        BeforeExecution = options.BeforeExecution;
        AfterExecution = options.AfterExecution;
        OnFailure = options.OnFailure;

        for (int i = 0; i < options.Workers; i++)
            _workers.Add(Task.Run(() => WorkerLoopAsync(_cts.Token)));
    }

    public void Enqueue(Func<Task> asyncAction, int weight = 0)
    {
        _queue.Enqueue(asyncAction, weight);
        _signal.Release();
        
        if (_signal.CurrentCount == 0)
            EnsureElasticWorkers();
    }

    public int PendingWorkCount()
        => _queue.PendingWorkCount;

    public int CurrentElasticWorkersCount()
        => _elasticWorkersCount;

    public void Dispose()
    {
        _cts.Cancel();
        try
        {
            if (_workers != null) Task.WaitAll(_workers.ToArray());
            if (_elasticWorkers != null) Task.WaitAll(_elasticWorkers.ToArray());
        }
        catch (AggregateException) { }
        finally
        {
            _cts.Dispose();
            _signal.Dispose();
        }
    }

    #region PRIVATE
    private void EnsureElasticWorkers()
    {
        if (_queue.IsEmpty || _elasticWorkersCount >= Options.MaxElasticWorkers)
            return;

        if (Interlocked.CompareExchange(ref _elasticWorkersCount, 
            _elasticWorkersCount + 1, _elasticWorkersCount) < Options.MaxElasticWorkers)
        {
            var workerTask = Task.Run(() => ElasticWorkerLoopAsync(_cts.Token));
            _elasticWorkers.Add(workerTask);
        }
        else
        {
            Interlocked.Decrement(ref _elasticWorkersCount);
        }
    }

    private async Task WorkerLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                await _signal.WaitAsync(token);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            if (_queue.TryDequeue(out var task))
            {
                await ExecuteTaskAsync(task);
            }
        }
    }

    private async Task ElasticWorkerLoopAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await _signal.WaitAsync(Options.ElasticWorkersTimeout, token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (TimeoutException)
                {
                    break;
                }

                if (_queue.TryDequeue(out var task))
                {
                    await ExecuteTaskAsync(task);
                }
                else
                {
                    break;
                }
            }
        }
        finally
        {
            Interlocked.Decrement(ref _elasticWorkersCount);
        }
    }

    private async Task ExecuteTaskAsync(TaskItem<Func<Task>> task)
    {
        try
        {
            BeforeExecution?.Invoke(new TaskInfo(task.Id, task.Weight, true));
            await task.Entity.Invoke();
            AfterExecution?.Invoke(new TaskInfo(task.Id, task.Weight, true));
        }
        catch (Exception ex)
        {
            OnFailure?.Invoke(new TaskInfo(task.Id, task.Weight, true), ex);
        }
    }
    #endregion
}