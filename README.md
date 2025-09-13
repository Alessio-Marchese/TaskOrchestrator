# TaskOrchestrator 📊

**TaskOrchestrator** is a high-performance, production-ready utility for managing concurrent tasks.
It provides flexible **Sync, Async, and Hybrid pools** with advanced optimizations for memory efficiency and thread safety.

---

## 🌱 What is a Pool?

A **Pool** is an execution ecosystem defined by:

* 🔧 **Configuration** (set at creation time)
* 🗂 **Prioritized Queue** (where tasks are stored and scheduled)

> 💡 A **Hybrid Pool** is simply a wrapper that combines **1 Sync Pool** and **1 Async Pool**.

---

## ⚙️ Pool Configuration

Each pool can be customized with:

* **Fixed Workers**
  Workers always running, constantly executing tasks from the queue.

* **Elastic Workers**

  * Spawned when all fixed workers are busy
  * Automatically disposed after a configurable idle time
  * Disabled if set to `0`

* **Lifecycle Events**
  Three hooks are available:

  * `BeforeExecution<TaskInfo>?`
  * `AfterExecution<TaskInfo>?`
  * `OnFailure<TaskInfo, Exception>?`

  Each event provides a `TaskInfo` object:

  * `Id`
  * `Weight`
  * `IsAsync`
  * In case of `OnFailure`, the thrown `Exception`

---

## 💻 Example

```csharp
var hybridPool = HybridPool.Create(
    Options.GetElasticOptions(
        2, // Workers
        5, // MaxElasticWorkers
        TimeSpan.FromSeconds(10), // 10s Idle
        (info) => Console.WriteLine($"Before Executing Task:{info.id}"), // BeforeExecution Hook
        (info) => Console.WriteLine($"After Executing Task:{info.id}"), // AfterExecution Hook
        (info, ex) => Console.WriteLine($"Task: {info.id} Just failed :( \n{ex.StackTrace}") // OnFailure Hook
    )
);

//Enqueue Sync
hybridPool.Enqueue(
    () =>
    {
        Thread.Sleep(100); // Do stuff
    }, i /*Weight*/);

//Enqueue Async
hybridPool.Enqueue(
    async () =>
    {
        await Task.Delay(100); // Do stuff
    }, i /*Weight*/);
```

### Breakdown

* 🔄 **Hybrid Pool** → internally holds **1 Sync Pool** + **1 Async Pool**
* 🧑‍💻 **2 Fixed Workers** per pool → always running
* ⚡ **5 Elastic Workers** per pool → created on demand, disposed after **10s idle**
* 🎯 **3 Lifecycle Events** → triggered before, after, and on failure

---

## 🚀 Motivation

This project started as a **small but useful experiment** in concurrency management.
It’s still in **early development**, but the goal is to build something:

* ✅ Solid
* ✅ User-friendly
* ✅ Practical
* ✅ **Zero Allocations**: Synchronous tasks use struct-based implementation
* ✅ **Memory Efficiency**: Automatic cleanup of elastic workers
* ✅ **Thread Safety**: Optimized lock contention and atomic operations
* ✅ **High Performance**: Linear scalability with minimal GC pressure

---

## 📊 Performance

TaskOrchestrator delivers exceptional performance with:

* **100% reduction** in memory allocations for synchronous tasks
* **Linear scalability** from 100 to 10,000+ tasks
* **Minimal lock contention** with atomic operations
* **Zero memory leaks** with automatic resource cleanup

### Benchmark Results
- **CPU-Bound 1000 sync tasks**: 1.359 ms (zero allocations)
- **CPU-Bound 1000 async tasks**: 9.877 ms (minimal allocations)
---

## 🛠️ Development Status

**Current Version**: 1.0.1

### Recently Completed
* ✅ **Memory optimization** with struct-based task implementation
* ✅ **Lock contention reduction** with atomic operations
* ✅ **Memory leak elimination** in elastic workers
* ✅ **Comprehensive benchmarking** system
* ✅ **Performance validation** with realistic workloads


## 🔮 Future Improvements

Planned features include:

* **Task-level lifecycle configuration**
  Override global pool events for specific tasks.

* **Dynamic Task Prioritization (Aging)**
  Prevent low-priority tasks from being starved by increasing their priority over time.


