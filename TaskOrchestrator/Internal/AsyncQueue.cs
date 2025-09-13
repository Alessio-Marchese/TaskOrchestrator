using TaskOrchestrator.Utils;

namespace TaskOrchestrator.Internal;

internal class AsyncQueue
{
    private readonly PriorityQueue<TaskItem<Func<Task>>, int> _asyncQueue = new();
    private readonly object _lock = new();

    public void Enqueue(Func<Task> entity, int weight)
    {
        lock (_lock)
        {
            _asyncQueue.Enqueue(new TaskItem<Func<Task>>(entity, weight), -weight);
        }
    }

    public bool TryDequeue(out TaskItem<Func<Task>> item)
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
        get { lock (_lock) return _asyncQueue.Count; }
    }
}
