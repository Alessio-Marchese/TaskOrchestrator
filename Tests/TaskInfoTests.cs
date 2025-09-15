using TaskOrchestrator.Utils;

namespace Tests;

public class TaskInfoTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateTaskInfo()
    {
        var taskInfo = new TaskInfo(1, 5, true);
        
        Assert.Equal(1, taskInfo.Id);
        Assert.Equal(5, taskInfo.Weight);
        Assert.True(taskInfo.IsAsync);
    }

    [Fact]
    public void Constructor_WithSyncTask_ShouldSetIsAsyncToFalse()
    {
        var taskInfo = new TaskInfo(2, 3, false);
        
        Assert.Equal(2, taskInfo.Id);
        Assert.Equal(3, taskInfo.Weight);
        Assert.False(taskInfo.IsAsync);
    }

    [Fact]
    public void Constructor_WithZeroWeight_ShouldAcceptZeroWeight()
    {
        var taskInfo = new TaskInfo(1, 0, true);
        
        Assert.Equal(0, taskInfo.Weight);
    }

    [Fact]
    public void Constructor_WithNegativeWeight_ShouldAcceptNegativeWeight()
    {
        var taskInfo = new TaskInfo(1, -1, true);
        
        Assert.Equal(-1, taskInfo.Weight);
    }
}

