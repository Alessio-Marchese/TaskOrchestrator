using TaskOrchestrator.Utils;

namespace TaskOrchestrator.Internal;

public class SyncQueue
{
    private readonly PriorityQueue<TaskItem<Action>, int> _syncQueue = new();
    private readonly object _lock = new();

    public void Enqueue(Action entity, int weight)
    {
        lock (_lock)
        {
            _syncQueue.Enqueue(new TaskItem<Action>(entity, weight), -weight);
        }
    }

    public bool TryDequeue(out TaskItem<Action> item)
    {
        lock (_lock)
        {
            if (_syncQueue.Count > 0)
            {
                item = _syncQueue.Dequeue();
                return true;
            }
            item = default;
            return false;
        }
    }

    public bool IsEmpty
    {
        get { lock (_lock) return _syncQueue.Count == 0; }
    }

    public int PendingWorkCount
    {
        get { lock (_lock) return _syncQueue.Count; }
    }
}
