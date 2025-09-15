namespace TaskOrchestrator.Utils;

public class TaskInfo
{
    public int Id { get; private set; }
    public int Weight { get; private set; }
    public bool IsAsync { get; private set; }

    public TaskInfo(int id, int weight, bool isAsync)
    {
        Id = id;
        Weight = weight;
        IsAsync = isAsync;
    }
}
