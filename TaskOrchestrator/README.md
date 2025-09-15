# TaskOrchestrator 📊

**TaskOrchestrator** is a lightweight utility for managing concurrent tasks.
It provides flexible **Sync, Async, and Hybrid pools**, each with configurable behaviors.

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

> I’m still learning the **Task Parallel Library (TPL)** and iterating daily to improve stability, usability, and performance.

---

## Currently working on

* **NuGet Distribution**
  
* **GitHub Actions to learn how to CI/CD the package**

---

## 🧪 Testing

TaskOrchestrator includes a comprehensive test suite with covering:

### Test Coverage
* ✅ **Factory Methods** - Pool creation with different configurations
* ✅ **Async Task Execution** - Task enqueueing and execution
* ✅ **Sync Action Execution** - Action enqueueing and execution
* ✅ **Pending Work Count** - Queue monitoring and statistics
* ✅ **Elastic Workers** - Dynamic worker management
* ✅ **Callback System** - Lifecycle event handling
* ✅ **Concurrent Execution** - Multi-threaded task processing
* ✅ **Weight Priority** - Task prioritization system
* ✅ **Resource Management** - Proper disposal and cleanup
* ✅ **Edge Cases** - Error handling and boundary conditions
* ✅ **Options Validation** - Configuration parameter validation
* ✅ **TaskInfo & TaskItem** - Data structure functionality

### Running Tests
```bash
cd Tests
dotnet test --verbosity normal
```

### Test-Driven Development
The test suite ensures:
- **Correctness** of the hybrid pool logic
- **Thread safety** in concurrent scenarios
- **Resource management** and proper cleanup
- **Configuration validation** and error handling
- **Performance characteristics** under load

---

## 🛠️ Development Status

**Current Version**: 1.0.2

### Recently Completed
* ✅ **Memory optimization** with struct-based task implementation
* ✅ **Lock contention reduction** with atomic operations
* ✅ **Memory leak elimination** in elastic workers
* ✅ **Comprehensive benchmarking** system
* ✅ **Performance validation** with realistic workloads
* ✅ **Complete test suite** with 57 unit tests
* ✅ **Test-driven development** approach

## 🔮 Future Improvements

Planned features include:

* **Task-level lifecycle configuration**
  Override global pool events for specific tasks.

* **Dynamic Task Prioritization (Aging)**
  Prevent low-priority tasks from being starved by increasing their priority over time.


