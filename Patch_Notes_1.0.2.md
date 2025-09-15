## 🧪 Added Testing + Minor fixes

### Fixes
* Modified Async and Sync pool to public
* Also modified visibility of Async and Sync pool from inside HybridPool
* TaskInfo properties are now read only
* TaskItem now accept only not null Func<Task> or Action
* Changed visibility of hooks to private

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