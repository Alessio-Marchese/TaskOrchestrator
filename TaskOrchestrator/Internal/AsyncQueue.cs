using TaskOrchestrator.Utils;

namespace TaskOrchestrator.Internal;

internal class AsyncQueue
{
    private readonly PriorityQueue<Identified<Func<Task>>, int> _asyncQueue = new();
    private readonly object _lock = new();

    public void Enqueue(Identified<Func<Task>> item, int priority)
    {
        lock (_lock)
        {
            _asyncQueue.Enqueue(item, -priority);
        }
    }
    public bool TryDequeue(out Identified<Func<Task>>? item)
    {
        lock (_lock)
        {
            if (_asyncQueue.Count > 0)
            {
                item = _asyncQueue.Dequeue();
                return true;
            }
            item = default;
            return false;
        }
    }
    public bool IsEmpty
    {
        get { lock (_lock) return _asyncQueue.Count == 0; }
    }

    public int PendingWorkCount
    {
        get { return _asyncQueue.Count; }
    }
}
