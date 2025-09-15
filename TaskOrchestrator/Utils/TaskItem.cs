namespace TaskOrchestrator.Utils;

public readonly struct TaskItem<T> where T : notnull
{
    public readonly T Entity;
    public readonly int Weight;
    public readonly int Id;

    private static int _idCounter;
    private static int NextId() => Interlocked.Increment(ref _idCounter);

    public TaskItem(T entity, int weight)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        if (entity is not Action && entity is not Func<Task>)
            throw new ArgumentException("Only Action or Func<Task> are allowed.", nameof(entity));

        Entity = entity;
        Weight = weight;
        Id = NextId();
    }
}

