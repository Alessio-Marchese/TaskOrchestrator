using TaskOrchestrator.Utils;

namespace Tests;

public class OptionsTests
{
    #region GetBasicOptions Tests

    [Fact]
    public void GetBasicOptions_WithValidWorkers_ShouldCreateOptions()
    {
        var options = Options.GetBasicOptions(2);
        
        Assert.Equal(2, options.Workers);
        Assert.Equal(0, options.MaxElasticWorkers);
        Assert.Equal(TimeSpan.Zero, options.ElasticWorkersTimeout);
        Assert.False(options.IsElastic());
    }

    [Fact]
    public void GetBasicOptions_WithZeroWorkers_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Options.GetBasicOptions(0));
    }

    [Fact]
    public void GetBasicOptions_WithNegativeWorkers_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Options.GetBasicOptions(-1));
    }

    [Fact]
    public void GetBasicOptions_WithCallbacks_ShouldSetCallbacks()
    {  
        var options = Options.GetBasicOptions(1,
            beforeExecution: (taskInfo) => Console.WriteLine("BeforeExecution"),
            afterExecution: (taskInfo) => Console.WriteLine("AfterExecution"),
            onFailure: (taskInfo, ex) => Console.WriteLine("OnFailure"));
        
        Assert.NotNull(options.BeforeExecution);
        Assert.NotNull(options.AfterExecution);
        Assert.NotNull(options.OnFailure);
    }

    #endregion

    #region GetElasticOptions Tests

    [Fact]
    public void GetElasticOptions_WithValidParameters_ShouldCreateElasticOptions()
    {
        var timeout = TimeSpan.FromSeconds(5);
        var options = Options.GetElasticOptions(2, 3, timeout);
        
        Assert.Equal(2, options.Workers);
        Assert.Equal(3, options.MaxElasticWorkers);
        Assert.Equal(timeout, options.ElasticWorkersTimeout);
        Assert.True(options.IsElastic());
    }

    [Fact]
    public void GetElasticOptions_WithZeroMaxElasticWorkers_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => 
            Options.GetElasticOptions(1, 0, TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public void GetElasticOptions_WithNegativeMaxElasticWorkers_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => 
            Options.GetElasticOptions(1, -1, TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public void GetElasticOptions_WithZeroTimeout_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => 
            Options.GetElasticOptions(1, 1, TimeSpan.Zero));
    }

    [Fact]
    public void GetElasticOptions_WithNegativeTimeout_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => 
            Options.GetElasticOptions(1, 1, TimeSpan.FromMilliseconds(-1)));
    }

    [Fact]
    public void GetElasticOptions_WithCallbacks_ShouldSetCallbacks()
    {
        var options = Options.GetElasticOptions(1, 1, TimeSpan.FromSeconds(1),
            beforeExecution: (taskInfo) => Console.WriteLine("BeforeExecution"),
            afterExecution: (taskInfo) => Console.WriteLine("AfterExecution"),
            onFailure: (taskInfo, ex) => Console.WriteLine("OnFailure"));
        
        Assert.NotNull(options.BeforeExecution);
        Assert.NotNull(options.AfterExecution);
        Assert.NotNull(options.OnFailure);
    }

    #endregion

    #region IsElastic Tests

    [Fact]
    public void IsElastic_WithBasicOptions_ShouldReturnFalse()
    {
        var options = Options.GetBasicOptions(1);
        
        Assert.False(options.IsElastic());
    }

    [Fact]
    public void IsElastic_WithElasticOptions_ShouldReturnTrue()
    {
        var options = Options.GetElasticOptions(1, 1, TimeSpan.FromSeconds(1));
        
        Assert.True(options.IsElastic());
    }

    #endregion
}

