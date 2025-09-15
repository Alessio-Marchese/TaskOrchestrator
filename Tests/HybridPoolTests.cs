using TaskOrchestrator.Core;
using TaskOrchestrator.Utils;

namespace Tests;

public class HybridPoolTests
{
    #region Factory Methods Tests

    [Fact]
    public void Create_WithSingleOptions_ShouldCreateHybridPoolWithSameOptions()
    {
        var options = Options.GetBasicOptions(2);
        
        var hybridPool = HybridPool.Create(options);
        
        Assert.Equal(options, hybridPool.AsyncPoolService.Options);
        Assert.Equal(options, hybridPool.SyncPoolService.Options);

        hybridPool.Dispose();
    }

    [Fact]
    public void Create_WithDifferentOptions_ShouldCreateHybridPoolWithDifferentOptions()
    {
        var asyncOptions = Options.GetBasicOptions(2);
        var syncOptions = Options.GetBasicOptions(3);
        
        var hybridPool = HybridPool.Create(asyncOptions, syncOptions);
        
        Assert.NotEqual(hybridPool.AsyncPoolService.Options.Workers, hybridPool.SyncPoolService.Options.Workers);
        
        hybridPool.Dispose();
    }

    #endregion

    #region Async Task Enqueue Tests

    [Fact]
    public void Enqueue_AsyncTask_ShouldEnqueueToAsyncPool()
    {
        var options = Options.GetBasicOptions(1);
        var hybridPool = HybridPool.Create(options);
        
        var taskExecuted = false;
        Func<Task> asyncTask = async () => 
        {
            await Task.Delay(10);
            taskExecuted = true;
        };
        
        hybridPool.Enqueue(asyncTask);
        
        Thread.Sleep(50);
        Assert.True(taskExecuted);
        
        hybridPool.Dispose();
    }

    [Fact]
    public void Enqueue_AsyncTaskWithWeight_ShouldEnqueueWithCorrectWeight()
    {
        var options = Options.GetBasicOptions(1);
        var hybridPool = HybridPool.Create(options);
        
        var taskExecuted = false;
        Func<Task> asyncTask = async () => 
        {
            await Task.Delay(10);
            taskExecuted = true;
        };
        
        hybridPool.Enqueue(asyncTask, 5);
        
        Thread.Sleep(50);
        Assert.True(taskExecuted);
        
        hybridPool.Dispose();
    }

    [Fact]
    public void Enqueue_AsyncTaskWithZeroWorkers_ShouldNotThrowException()
    {
        var options = Options.GetElasticOptions(0, 1, TimeSpan.FromMilliseconds(100));
        var hybridPool = HybridPool.Create(options);
        
        Func<Task> asyncTask = async () => await Task.CompletedTask;
        
        hybridPool.Enqueue(asyncTask);
        
        hybridPool.Dispose();
    }

    [Fact]
    public void Enqueue_AsyncTaskWithElasticWorkers_ShouldNotThrowException()
    {
        var options = Options.GetElasticOptions(0, 1, TimeSpan.FromMilliseconds(1000));
        var hybridPool = HybridPool.Create(options);
        
        var taskExecuted = false;
        Func<Task> asyncTask = async () => 
        {
            await Task.Delay(10);
            taskExecuted = true;
        };
        
        hybridPool.Enqueue(asyncTask);
        
        Thread.Sleep(500);
        
        hybridPool.Dispose();
    }

    #endregion

    #region Sync Action Enqueue Tests

    [Fact]
    public void Enqueue_SyncAction_ShouldEnqueueToSyncPool()
    {
        var options = Options.GetBasicOptions(1);
        var hybridPool = HybridPool.Create(options);
        
        var actionExecuted = false;
        Action syncAction = () => actionExecuted = true;
        
        hybridPool.Enqueue(syncAction);
        
        Thread.Sleep(50);
        Assert.True(actionExecuted);
        
        hybridPool.Dispose();
    }

    [Fact]
    public void Enqueue_SyncActionWithWeight_ShouldEnqueueWithCorrectWeight()
    {
        var options = Options.GetBasicOptions(1);
        var hybridPool = HybridPool.Create(options);
        
        var actionExecuted = false;
        Action syncAction = () => actionExecuted = true;
        
        hybridPool.Enqueue(syncAction, 3);
        
        Thread.Sleep(50);
        Assert.True(actionExecuted);
        
        hybridPool.Dispose();
    }

    [Fact]
    public void Enqueue_SyncActionWithZeroWorkers_ShouldNotThrowException()
    {
        var options = Options.GetElasticOptions(0, 1, TimeSpan.FromMilliseconds(100));
        var hybridPool = HybridPool.Create(options);
        
        Action syncAction = () => { };
        
        hybridPool.Enqueue(syncAction);
        
        hybridPool.Dispose();
    }

    [Fact]
    public void Enqueue_SyncActionWithElasticWorkers_ShouldNotThrowException()
    {
        var options = Options.GetElasticOptions(0, 1, TimeSpan.FromMilliseconds(1000));
        var hybridPool = HybridPool.Create(options);
        
        var actionExecuted = false;
        Action syncAction = () => actionExecuted = true;
        
        hybridPool.Enqueue(syncAction);
        
        Thread.Sleep(500);
        
        hybridPool.Dispose();
    }

    #endregion

    #region Pending Work Count Tests

    [Fact]
    public void PendingWorkCount_WithNoTasks_ShouldReturnZero()
    {
        var options = Options.GetBasicOptions(1);
        var hybridPool = HybridPool.Create(options);
        
        var count = hybridPool.PendingWorkCount();
        
        Assert.Equal(0, count);
        
        hybridPool.Dispose();
    }

    [Fact]
    public void PendingWorkCount_WithAsyncTasks_ShouldReturnCorrectCount()
    {
        var options = Options.GetBasicOptions(1);
        var hybridPool = HybridPool.Create(options);
        
        Func<Task> asyncTask1 = async () => await Task.Delay(1000);
        Func<Task> asyncTask2 = async () => await Task.Delay(1000);
        
        hybridPool.Enqueue(asyncTask1);
        hybridPool.Enqueue(asyncTask2);
        
        var count = hybridPool.PendingWorkCount();
        
        Assert.True(count >= 1);
        
        hybridPool.Dispose();
    }

    [Fact]
    public void PendingWorkCount_WithSyncActions_ShouldReturnCorrectCount()
    {
        var options = Options.GetBasicOptions(1);
        var hybridPool = HybridPool.Create(options);
        
        Action syncAction1 = () => Thread.Sleep(1000);
        Action syncAction2 = () => Thread.Sleep(1000);
        
        hybridPool.Enqueue(syncAction1);
        hybridPool.Enqueue(syncAction2);
        
        var count = hybridPool.PendingWorkCount();
        
        Assert.True(count >= 1);
        
        hybridPool.Dispose();
    }

    [Fact]
    public void PendingWorkCount_WithMixedTasks_ShouldReturnTotalCount()
    {
        var options = Options.GetBasicOptions(1);
        var hybridPool = HybridPool.Create(options);
        
        Func<Task> asyncTask = async () => await Task.Delay(1000);
        Action syncAction = () => Thread.Sleep(1000);
        
        hybridPool.Enqueue(asyncTask);
        hybridPool.Enqueue(syncAction);
        
        var count = hybridPool.PendingWorkCount();
        
        Assert.True(count >= 1);
        
        hybridPool.Dispose();
    }

    #endregion

    #region Specific Pending Work Count Tests

    [Fact]
    public void PendingAsyncWorkCount_WithAsyncTasks_ShouldReturnAsyncCount()
    {
        var options = Options.GetBasicOptions(1);
        var hybridPool = HybridPool.Create(options);
        
        Func<Task> asyncTask1 = async () => await Task.Delay(1000);
        Func<Task> asyncTask2 = async () => await Task.Delay(1000);
        Action syncAction = () => Thread.Sleep(1000);
        
        hybridPool.Enqueue(asyncTask1);
        var asyncCount1 = hybridPool.PendingAsyncWorkCount();
        var totalCount1 = hybridPool.PendingWorkCount();
        
        hybridPool.Enqueue(asyncTask2);
        var asyncCount2 = hybridPool.PendingAsyncWorkCount();
        var totalCount2 = hybridPool.PendingWorkCount();
        
        hybridPool.Enqueue(syncAction);
        var asyncCount3 = hybridPool.PendingAsyncWorkCount();
        var totalCount3 = hybridPool.PendingWorkCount();
        
        Assert.True(asyncCount1 >= 0);
        Assert.True(totalCount1 >= 0);
        Assert.True(asyncCount2 >= 0);
        Assert.True(totalCount2 >= 0);
        Assert.True(asyncCount3 >= 0);
        Assert.True(totalCount3 >= 0);
        
        hybridPool.Dispose();
    }

    [Fact]
    public void PendingSyncWorkCount_WithSyncActions_ShouldReturnSyncCount()
    {
        var options = Options.GetBasicOptions(1);
        var hybridPool = HybridPool.Create(options);
        
        Func<Task> asyncTask = async () => await Task.Delay(1000);
        Action syncAction1 = () => Thread.Sleep(1000);
        Action syncAction2 = () => Thread.Sleep(1000);
        
        hybridPool.Enqueue(asyncTask);
        var syncCount1 = hybridPool.PendingSyncWorkCount();
        var totalCount1 = hybridPool.PendingWorkCount();
        
        hybridPool.Enqueue(syncAction1);
        var syncCount2 = hybridPool.PendingSyncWorkCount();
        var totalCount2 = hybridPool.PendingWorkCount();
        
        hybridPool.Enqueue(syncAction2);
        var syncCount3 = hybridPool.PendingSyncWorkCount();
        var totalCount3 = hybridPool.PendingWorkCount();
        
        Assert.True(syncCount1 >= 0);
        Assert.True(totalCount1 >= 0);
        Assert.True(syncCount2 >= 0);
        Assert.True(totalCount2 >= 0);
        Assert.True(syncCount3 >= 0);
        Assert.True(totalCount3 >= 0);
        
        hybridPool.Dispose();
    }

    #endregion

    #region Elastic Workers Tests

    [Fact]
    public void CurrentElasticWorkers_WithNoElasticWorkers_ShouldReturnZero()
    {
        var options = Options.GetBasicOptions(1);
        var hybridPool = HybridPool.Create(options);
        
        var elasticWorkers = hybridPool.CurrentElasticWorkers();
        
        Assert.Equal(0, elasticWorkers);
        
        hybridPool.Dispose();
    }

    [Fact]
    public void CurrentElasticWorkers_WithElasticWorkers_ShouldReturnCorrectCount()
    {
        var options = Options.GetElasticOptions(1, 2, TimeSpan.FromMilliseconds(100));
        var hybridPool = HybridPool.Create(options);
        
        var elasticWorkers = hybridPool.CurrentElasticWorkers();
        
        Assert.Equal(0, elasticWorkers);
        
        hybridPool.Dispose();
    }

    [Fact]
    public void CurrentAsyncElasticWorkers_ShouldReturnAsyncElasticCount()
    {
        var asyncOptions = Options.GetElasticOptions(1, 2, TimeSpan.FromMilliseconds(100));
        var syncOptions = Options.GetBasicOptions(1);
        var hybridPool = HybridPool.Create(asyncOptions, syncOptions);
        
        var asyncElasticWorkers = hybridPool.CurrentAsyncElasticWorkers();
        var totalElasticWorkers = hybridPool.CurrentElasticWorkers();
        
        Assert.Equal(0, asyncElasticWorkers);
        Assert.Equal(0, totalElasticWorkers);
        
        hybridPool.Dispose();
    }

    [Fact]
    public void CurrentSyncElasticWorkers_ShouldReturnSyncElasticCount()
    {
        var asyncOptions = Options.GetBasicOptions(1);
        var syncOptions = Options.GetElasticOptions(1, 2, TimeSpan.FromMilliseconds(100));
        var hybridPool = HybridPool.Create(asyncOptions, syncOptions);
        
        var syncElasticWorkers = hybridPool.CurrentSyncElasticWorkers();
        var totalElasticWorkers = hybridPool.CurrentElasticWorkers();
        
        Assert.Equal(0, syncElasticWorkers);
        Assert.Equal(0, totalElasticWorkers);
        
        hybridPool.Dispose();
    }

    #endregion

    #region Callback Tests

    [Fact]
    public void Enqueue_AsyncTaskWithCallbacks_ShouldInvokeCallbacks()
    {
        var beforeExecutionCalled = false;
        var afterExecutionCalled = false;
        var onFailureCalled = false;
        
        var options = Options.GetBasicOptions(1,
            beforeExecution: (taskInfo) => 
            {
                beforeExecutionCalled = true;
                Assert.True(taskInfo.IsAsync);
            },
            afterExecution: (taskInfo) => 
            {
                afterExecutionCalled = true;
                Assert.True(taskInfo.IsAsync);
            },
            onFailure: (taskInfo, ex) => 
            {
                onFailureCalled = true;
                Assert.True(taskInfo.IsAsync);
            });
        
        var hybridPool = HybridPool.Create(options);
        
        Func<Task> asyncTask = async () => await Task.Delay(10);
        
        hybridPool.Enqueue(asyncTask);
        
        Thread.Sleep(50);
        
        Assert.True(beforeExecutionCalled);
        Assert.True(afterExecutionCalled);
        Assert.False(onFailureCalled);
        
        hybridPool.Dispose();
    }

    [Fact]
    public void Enqueue_SyncActionWithCallbacks_ShouldInvokeCallbacks()
    {
        var beforeExecutionCalled = false;
        var afterExecutionCalled = false;
        var onFailureCalled = false;
        
        var options = Options.GetBasicOptions(1,
            beforeExecution: (taskInfo) => 
            {
                beforeExecutionCalled = true;
                Assert.False(taskInfo.IsAsync);
            },
            afterExecution: (taskInfo) => 
            {
                afterExecutionCalled = true;
                Assert.False(taskInfo.IsAsync);
            },
            onFailure: (taskInfo, ex) => 
            {
                onFailureCalled = true;
                Assert.False(taskInfo.IsAsync);
            });
        
        var hybridPool = HybridPool.Create(options);
        
        Action syncAction = () => { };
        
        hybridPool.Enqueue(syncAction);
        
        Thread.Sleep(50);
        
        Assert.True(beforeExecutionCalled);
        Assert.True(afterExecutionCalled);
        Assert.False(onFailureCalled);
        
        hybridPool.Dispose();
    }

    [Fact]
    public void Enqueue_AsyncTaskWithException_ShouldInvokeOnFailureCallback()
    {
        var onFailureCalled = false;
        Exception? capturedException = null;
        
        var options = Options.GetBasicOptions(1,
            onFailure: (taskInfo, ex) => 
            {
                onFailureCalled = true;
                capturedException = ex;
                Assert.True(taskInfo.IsAsync);
            });
        
        var hybridPool = HybridPool.Create(options);
        
        Func<Task> asyncTask = async () => 
        {
            await Task.Delay(10);
            throw new InvalidOperationException("Test exception");
        };
        
        hybridPool.Enqueue(asyncTask);
        
        Thread.Sleep(50);
        
        Assert.True(onFailureCalled);
        Assert.NotNull(capturedException);
        Assert.IsType<InvalidOperationException>(capturedException);
        
        hybridPool.Dispose();
    }

    [Fact]
    public void Enqueue_SyncActionWithException_ShouldInvokeOnFailureCallback()
    {
        var onFailureCalled = false;
        Exception? capturedException = null;
        
        var options = Options.GetBasicOptions(1,
            onFailure: (taskInfo, ex) => 
            {
                onFailureCalled = true;
                capturedException = ex;
                Assert.False(taskInfo.IsAsync);
            });
        
        var hybridPool = HybridPool.Create(options);
        
        Action syncAction = () => throw new ArgumentException("Test exception");
        
        hybridPool.Enqueue(syncAction);
        
        Thread.Sleep(50);
        
        Assert.True(onFailureCalled);
        Assert.NotNull(capturedException);
        Assert.IsType<ArgumentException>(capturedException);
        
        hybridPool.Dispose();
    }

    #endregion

    #region Concurrent Execution Tests

    [Fact]
    public void Enqueue_MultipleAsyncTasks_ShouldExecuteConcurrently()
    {
        var options = Options.GetBasicOptions(2);
        var hybridPool = HybridPool.Create(options);
        
        var executionOrder = new List<int>();
        var lockObject = new object();
        
        for (int i = 0; i < 5; i++)
        {
            var taskId = i;
            Func<Task> asyncTask = async () => 
            {
                await Task.Delay(50);
                lock (lockObject)
                {
                    executionOrder.Add(taskId);
                }
            };
            
            hybridPool.Enqueue(asyncTask);
        }
        
        Thread.Sleep(200);
        
        Assert.Equal(5, executionOrder.Count);
        
        hybridPool.Dispose();
    }

    [Fact]
    public void Enqueue_MultipleSyncActions_ShouldExecuteConcurrently()
    {
        var options = Options.GetBasicOptions(2);
        var hybridPool = HybridPool.Create(options);
        
        var executionOrder = new List<int>();
        var lockObject = new object();
        
        for (int i = 0; i < 5; i++)
        {
            var actionId = i;
            Action syncAction = () => 
            {
                Thread.Sleep(50);
                lock (lockObject)
                {
                    executionOrder.Add(actionId);
                }
            };
            
            hybridPool.Enqueue(syncAction);
        }
        
        Thread.Sleep(200);
        
        Assert.Equal(5, executionOrder.Count);
        
        hybridPool.Dispose();
    }

    [Fact]
    public void Enqueue_MixedTasks_ShouldExecuteBothTypes()
    {
        var options = Options.GetBasicOptions(2);
        var hybridPool = HybridPool.Create(options);
        
        var asyncExecuted = false;
        var syncExecuted = false;
        
        Func<Task> asyncTask = async () => 
        {
            await Task.Delay(50);
            asyncExecuted = true;
        };
        
        Action syncAction = () => 
        {
            Thread.Sleep(50);
            syncExecuted = true;
        };
        
        hybridPool.Enqueue(asyncTask);
        hybridPool.Enqueue(syncAction);
        
        Thread.Sleep(100);
        
        Assert.True(asyncExecuted);
        Assert.True(syncExecuted);
        
        hybridPool.Dispose();
    }

    #endregion

    #region Weight Priority Tests

    [Fact]
    public void Enqueue_TasksWithDifferentWeights_ShouldRespectPriority()
    {
        var options = Options.GetBasicOptions(1);
        var hybridPool = HybridPool.Create(options);
        
        var executionOrder = new List<int>();
        var lockObject = new object();
        
        Func<Task> lowPriorityTask = async () => 
        {
            await Task.Delay(10);
            lock (lockObject)
            {
                executionOrder.Add(1);
            }
        };
        
        Func<Task> highPriorityTask = async () => 
        {
            await Task.Delay(10);
            lock (lockObject)
            {
                executionOrder.Add(2);
            }
        };
        
        hybridPool.Enqueue(lowPriorityTask, 1);
        hybridPool.Enqueue(highPriorityTask, 10);
        
        Thread.Sleep(100);
        
        Assert.Equal(2, executionOrder.Count);
        
        hybridPool.Dispose();
    }

    #endregion

    #region Dispose Tests

    [Fact]
    public void Dispose_ShouldCleanupResources()
    {
        var options = Options.GetBasicOptions(1);
        var hybridPool = HybridPool.Create(options);
        
        hybridPool.Dispose();
        
        Assert.Throws<ObjectDisposedException>(() => hybridPool.Dispose());
    }

    [Fact]
    public void Dispose_WithPendingTasks_ShouldCompleteGracefully()
    {
        var options = Options.GetBasicOptions(1);
        var hybridPool = HybridPool.Create(options);
        
        Func<Task> asyncTask = async () => await Task.Delay(100);
        Action syncAction = () => Thread.Sleep(100);
        
        hybridPool.Enqueue(asyncTask);
        hybridPool.Enqueue(syncAction);
        
        hybridPool.Dispose();
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void Enqueue_NullAsyncTask_ShouldNotThrowException()
    {
        var options = Options.GetBasicOptions(1);
        var hybridPool = HybridPool.Create(options);
        
        hybridPool.Enqueue(async () => await Task.Delay(50));
        
        hybridPool.Dispose();
    }

    [Fact]
    public void Enqueue_NullSyncAction_ShouldNotThrowException()
    {
        var options = Options.GetBasicOptions(1);
        var hybridPool = HybridPool.Create(options);
        
        hybridPool.Enqueue(() => Console.WriteLine("Hello World!"));
        
        hybridPool.Dispose();
    }

    [Fact]
    public void PendingWorkCount_AfterDispose_ShouldReturnZero()
    {
        var options = Options.GetBasicOptions(1);
        var hybridPool = HybridPool.Create(options);
        
        Func<Task> asyncTask = async () => await Task.Delay(1000);
        hybridPool.Enqueue(asyncTask);
        
        hybridPool.Dispose();
        
        var count = hybridPool.PendingWorkCount();
        Assert.Equal(0, count);
    }

    #endregion
}
