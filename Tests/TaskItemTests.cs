using TaskOrchestrator.Utils;

namespace Tests;

public class TaskItemTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateTaskItem()
    {
        var entity = () => Console.WriteLine("Hello World!");
        var weight = 5;
        
        var taskItem = new TaskItem<Action>(entity, weight);
        
        Assert.Equal(entity, taskItem.Entity);
        Assert.Equal(weight, taskItem.Weight);
        Assert.True(taskItem.Id > 0);
    }

    [Fact]
    public void Constructor_WithZeroWeight_ShouldAcceptZeroWeight()
    {
        var entity = () => Console.WriteLine("Hello World!");
        var taskItem = new TaskItem<Action>(entity, 0);
        
        Assert.Equal(0, taskItem.Weight);
    }

    [Fact]
    public void Constructor_WithNegativeWeight_ShouldAcceptNegativeWeight()
    {
        var entity = () => Console.WriteLine("Hello World!");
        var taskItem = new TaskItem<Action>(entity, -1);
        
        Assert.Equal(-1, taskItem.Weight);
    }

    [Fact]
    public void Constructor_WithNullEntity_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new TaskItem<Action>(null!, 1));
    }

    [Fact]
    public void Id_ShouldBeUniqueForEachInstance()
    {
        var taskItem1 = new TaskItem<Action>(() => Console.WriteLine("Hello World!"), 1);
        var taskItem2 = new TaskItem<Action>(() => Console.WriteLine("Hello World!"), 1);
        var taskItem3 = new TaskItem<Action>(() => Console.WriteLine("Hello World!"), 1);
        
        Assert.NotEqual(taskItem1.Id, taskItem2.Id);
        Assert.NotEqual(taskItem2.Id, taskItem3.Id);
        Assert.NotEqual(taskItem1.Id, taskItem3.Id);
    }

    [Fact]
    public void Id_ShouldBeSequential()
    {
        var taskItem1 = new TaskItem<Action>(() => Console.WriteLine("Hello World!"), 1);
        var taskItem2 = new TaskItem<Action>(() => Console.WriteLine("Hello World!"), 1);
        var taskItem3 = new TaskItem<Action>(() => Console.WriteLine("Hello World!"), 1);

        Assert.True(taskItem2.Id > taskItem1.Id);
        Assert.True(taskItem3.Id > taskItem2.Id);
    }

    [Fact]
    public void Properties_ShouldBeReadable()
    {
        var entity = () => Console.WriteLine("Hello World!");
        var weight = 5;
        var taskItem = new TaskItem<Action>(entity, weight);

        Assert.Equal(entity, taskItem.Entity);
        Assert.Equal(weight, taskItem.Weight);
        Assert.True(taskItem.Id > 0);
    }

    [Fact]
    public void Constructor_WithDifferentTypes_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new TaskItem<int>(42, 1));
        Assert.Throws<ArgumentException>(() => new TaskItem<bool>(true, 2));
        Assert.Throws<ArgumentException>(() => new TaskItem<List<string>>(new List<string>(), 3));
    }
}

