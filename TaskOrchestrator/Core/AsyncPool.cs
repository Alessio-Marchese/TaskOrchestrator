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
    private readonly List<Task> _elasticWorkers = new();
    private readonly SemaphoreSlim _signal;
    private readonly AsyncQueue _queue = new();
    private readonly object _elasticLock = new();

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
        lock (_elasticLock)
        {
            _queue.Enqueue(new Identified<Func<Task>>(asyncAction, weight), weight);
            _signal.Release();
            if (_signal.CurrentCount == 0)
            {
                EnsureElasticWorkers();
            }
        }
    }

    public int PendingWorkCount()
        => _queue.PendingWorkCount;

    public int CurrentElasticWorkersCount()
        => _elasticWorkers.Count;

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
        if (_queue.IsEmpty || _elasticWorkers.Count >= Options.MaxElasticWorkers)
            return;

        lock (_elasticLock)
        {
            if (_elasticWorkers.Count < Options.MaxElasticWorkers && !_queue.IsEmpty)
            {
                var workerTask = Task.Run(() => ElasticWorkerLoopAsync(_cts.Token));
                _elasticWorkers.Add(workerTask);
            }
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
                if (task != null)
                    await ExecuteTaskAsync(task);
            }
        }
    }

    private async Task ElasticWorkerLoopAsync(CancellationToken token)
    {
        int? taskId = Task.CurrentId;

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
                if (task != null)
                    await ExecuteTaskAsync(task);
            }
            else
            {
                break;
            }
        }

        lock (_elasticLock)
        {
            if (taskId.HasValue)
            {
                _elasticWorkers.RemoveAll(t => t.Id == taskId.Value);
            }
        }
    }

    private async Task ExecuteTaskAsync(Identified<Func<Task>> task)
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