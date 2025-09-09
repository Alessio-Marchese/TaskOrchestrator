using TaskOrchestrator.Utils;

namespace TaskOrchestrator.Internal;

public class SyncQueue
{
    private readonly PriorityQueue<Identified<Action>, int> _syncQueue = new();
    private readonly object _lock = new();

    public void Enqueue(Identified<Action> item, int priority)
    {
        lock (_lock)
        {
            _syncQueue.Enqueue(item, -priority);
        }
    }

    public bool TryDequeue(out Identified<Action>? item)
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

    public int PendingWorkCount()
        => _syncQueue.Count;
}
