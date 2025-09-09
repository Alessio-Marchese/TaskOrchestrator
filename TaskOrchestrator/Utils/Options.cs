namespace TaskOrchestrator.Utils;

public class Options
{
    public int Workers { get; private set; }
    public int MaxElasticWorkers { get; private set; }
    public TimeSpan ElasticWorkersTimeout { get; private set; }

    public Action<TaskInfo>? BeforeExecution { get; set; }
    public Action<TaskInfo>? AfterExecution { get; set; }
    public Action<TaskInfo, Exception>? OnFailure { get; set; }

    private Options() { }

    /// <summary>
    /// With this function you can create a basic configuration.
    /// A basic configuration refers to a setup without elastic workers.
    /// </summary>
    /// <param name="workers"></param>
    /// <param name="beforeExecution"></param>
    /// <param name="afterExecution"></param>
    /// <param name="onFailure"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static Options GetBasicOptions(
        int workers,
        Action<TaskInfo>? beforeExecution = null,
        Action<TaskInfo>? afterExecution = null,
        Action<TaskInfo, Exception>? onFailure = null)
    {
        if (workers < 1)
            throw new ArgumentException("Must specify at least 1 worker in order to create a Basic configuration");

        return new Options
        {
            Workers = workers,
            BeforeExecution = beforeExecution,
            AfterExecution = afterExecution,
            OnFailure = onFailure
        };
    }

    /// <summary>
    /// With this function you can create an elastic configuration.
    /// </summary>
    /// <param name="workers"></param>
    /// <param name="maxElasticWorkers"></param>
    /// <param name="elasticWorkersTimeout"></param>
    /// <param name="beforeExecution"></param>
    /// <param name="afterExecution"></param>
    /// <param name="onFailure"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static Options GetElasticOptions(
        int workers,
        int maxElasticWorkers,
        TimeSpan elasticWorkersTimeout,
        Action<TaskInfo>? beforeExecution = null,
        Action<TaskInfo>? afterExecution = null,
        Action<TaskInfo, Exception>? onFailure = null)
    {
        if (maxElasticWorkers < 1)
            throw new ArgumentException("Must specify at least 1 elastic worker in order to create an Elastic configuration");
        if (elasticWorkersTimeout <= TimeSpan.Zero)
            throw new ArgumentException("Must not provide an empty or negative timeout in order to create an Elastic configuration");

        return new Options
        {
            Workers = workers,
            MaxElasticWorkers = maxElasticWorkers,
            ElasticWorkersTimeout = elasticWorkersTimeout,
            BeforeExecution = beforeExecution,
            AfterExecution = afterExecution,
            OnFailure = onFailure
        };
    }

    public bool IsElastic()
        => MaxElasticWorkers > 0;
}