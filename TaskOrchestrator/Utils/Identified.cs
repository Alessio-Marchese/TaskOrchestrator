namespace TaskOrchestrator.Utils;

public class Identified<T>
{
    private static int _idCounter;
    public int Id { get; } = Interlocked.Increment(ref _idCounter);

    public int Weight;
    public T Entity { get; private set; }

    public Identified(T entity, int weight)
    {
        Entity = entity;
        Weight = weight;
    }
}