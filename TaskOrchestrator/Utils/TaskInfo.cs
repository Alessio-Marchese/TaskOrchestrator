namespace TaskOrchestrator.Utils;

public class TaskInfo
{
    public int Id { get; set; }
    public int Weight { get; set; }
    public bool IsAsync { get; set; }

    public TaskInfo(int id, int weight, bool isAsync)
    {
        Id = id;
        Weight = weight;
        IsAsync = isAsync;
    }
}
