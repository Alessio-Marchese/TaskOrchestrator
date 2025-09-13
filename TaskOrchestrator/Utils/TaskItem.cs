namespace TaskOrchestrator.Utils;

public readonly struct TaskItem<T>
{
    public readonly T Entity;
    public readonly int Weight;
    public readonly int Id;

    private static int _idCounter;
    private static int NextId() => Interlocked.Increment(ref _idCounter);

    public TaskItem(T entity, int weight)
    {
        Entity = entity;
        Weight = weight;
        Id = NextId();
    }
}
