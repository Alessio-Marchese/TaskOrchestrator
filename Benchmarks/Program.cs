using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using TaskOrchestrator.Core;
using TaskOrchestrator.Utils;

namespace TaskOrchestrator.Benchmarks
{
    [MemoryDiagnoser]
    [ThreadingDiagnoser]
    [SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net80)]
    public class HybridPoolBenchmarks : IDisposable
    {
        private HybridPool _poolBasic;
        private HybridPool _poolElastic;
        private HybridPool _poolHighConcurrency;

        [GlobalSetup]
        public void Setup()
        {

            var warmupPool = HybridPool.Create(Options.GetBasicOptions(1));
            for (int i = 0; i < 100; i++)
                warmupPool.Enqueue(() => Math.Sqrt(i));
            warmupPool.Dispose();


            var basicOptions = Options.GetBasicOptions(workers: Environment.ProcessorCount);
            var elasticOptions = Options.GetElasticOptions(
                workers: Environment.ProcessorCount / 2,
                maxElasticWorkers: Environment.ProcessorCount,
                elasticWorkersTimeout: TimeSpan.FromMilliseconds(500));
            var highConcurrencyOptions = Options.GetElasticOptions(
                //Up to ProcessorCount times 3
                workers: Environment.ProcessorCount,
                maxElasticWorkers: Environment.ProcessorCount * 2,
                elasticWorkersTimeout: TimeSpan.FromMilliseconds(100));

            _poolBasic = HybridPool.Create(basicOptions);
            _poolElastic = HybridPool.Create(elasticOptions);
            _poolHighConcurrency = HybridPool.Create(highConcurrencyOptions);
        }

        #region CPU-Bound Workloads

        [Benchmark(Description = "CPU-Bound: 1000 sync tasks - BasicPool")]
        public void CpuBound_1000_Sync_Basic()
        {
            for (int i = 0; i < 1000; i++)
            {
                var value = i;
                _poolBasic.Enqueue(() => 
                {
                    var result = 0.0;
                    for (int j = 0; j < 100; j++)
                        result += Math.Sqrt(value + j);
                }, weight: i % 5);
            }
        }

        [Benchmark(Description = "CPU-Bound: 1000 async tasks - ElasticPool")]
        public async Task CpuBound_1000_Async_Elastic()
        {
            var tasks = new List<Task>();
            for (int i = 0; i < 1000; i++)
            {
                var value = i;
                _poolElastic.Enqueue(async () => 
                {
                    await Task.Run(() =>
                    {
                        var result = 0.0;
                        for (int j = 0; j < 50; j++)
                            result += Math.Sqrt(value + j);
                    });
                }, weight: i % 7);
            }

            await WaitForCompletion(_poolElastic);
        }

        #endregion

        #region I/O-Bound Workloads

        [Benchmark(Description = "I/O-Bound: 100 async tasks - BasicPool")]
        public async Task IoBound_100_Async_Basic()
        {
            for (int i = 0; i < 100; i++)
            {
                var value = i;
                _poolBasic.Enqueue(async () => 
                {
                    using var client = new HttpClient();
                    try
                    {
                        var response = await client.GetStringAsync($"https://httpbin.org/delay/0.001");
                        var processed = response.Length + value;
                    }
                    catch
                    {
                        await Task.Delay(1);
                    }
                }, weight: i % 3);
            }

            await WaitForCompletion(_poolBasic);
        }

        [Benchmark(Description = "I/O-Bound: 500 async tasks - ElasticPool")]
        public async Task IoBound_500_Async_Elastic()
        {
            for (int i = 0; i < 500; i++)
            {
                var value = i;
                _poolElastic.Enqueue(async () => 
                {
                    await Task.Delay(2); 
                    var result = value * value;
                }, weight: i % 9);
            }

            await WaitForCompletion(_poolElastic);
        }

        #endregion

        #region Mixed Workloads

        [Benchmark(Description = "Mixed: 200 sync + 200 async - HighConcurrency")]
        public async Task Mixed_400_Tasks_HighConcurrency()
        {
            for (int i = 0; i < 200; i++)
            {
                var value = i;
                _poolHighConcurrency.Enqueue(() => 
                {
                    var result = 0.0;
                    for (int j = 0; j < 25; j++)
                        result += Math.Sqrt(value + j);
                }, weight: i % 5);
            }

            for (int i = 0; i < 200; i++)
            {
                var value = i;
                _poolHighConcurrency.Enqueue(async () => 
                {
                    await Task.Delay(1);
                    var result = value * value;
                }, weight: i % 7);
            }

            await WaitForCompletion(_poolHighConcurrency);
        }

        #endregion

        #region Concurrent Enqueue Tests

        [Benchmark(Description = "Concurrent: 4 threads enqueue 250 tasks each")]
        public async Task Concurrent_1000_Tasks_4Threads()
        {
            var tasks = new List<Task>();
            var semaphore = new SemaphoreSlim(4);

            for (int thread = 0; thread < 4; thread++)
            {
                var threadId = thread;
                tasks.Add(Task.Run(async () =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        for (int i = 0; i < 250; i++)
                        {
                            var value = threadId * 250 + i;
                            _poolElastic.Enqueue(async () => 
                            {
                                await Task.Delay(1);
                                var result = Math.Sqrt(value);
                            }, weight: value % 11);
                        }
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }

            await Task.WhenAll(tasks);
            await WaitForCompletion(_poolElastic);
        }

        #endregion

        #region Stress Tests

        [Benchmark(Description = "Stress: 5000 sync tasks - BasicPool")]
        public void Stress_5000_Sync_Basic()
        {
            for (int i = 0; i < 5000; i++)
            {
                var value = i;
                _poolBasic.Enqueue(() => 
                {
                    var result = value % 1000;
                }, weight: i % 13);
            }
        }

        [Benchmark(Description = "Stress: 2000 async tasks - ElasticPool")]
        public async Task Stress_2000_Async_Elastic()
        {
            for (int i = 0; i < 2000; i++)
            {
                var value = i;
                _poolElastic.Enqueue(async () => 
                {
                    await Task.Delay(1);
                    var result = value % 500;
                }, weight: i % 17);
            }

            await WaitForCompletion(_poolElastic);
        }

        #endregion

        #region Helper Methods

        private async Task WaitForCompletion(HybridPool pool)
        {
            var timeout = TimeSpan.FromSeconds(30);
            var start = DateTime.UtcNow;
            
            while (pool.PendingWorkCount() > 0 && DateTime.UtcNow - start < timeout)
            {
                await Task.Delay(5);
            }
        }

        #endregion

        public void Dispose()
        {
            _poolBasic?.Dispose();
            _poolElastic?.Dispose();
            _poolHighConcurrency?.Dispose();
        }
    }

    [MemoryDiagnoser]
    [GcServer(true)]
    [GcConcurrent(true)]
    public class MemoryAnalysisBenchmarks : IDisposable
    {
        private HybridPool _pool;

        [GlobalSetup]
        public void Setup()
        {
            var options = Options.GetElasticOptions(
                workers: Environment.ProcessorCount,
                maxElasticWorkers: Environment.ProcessorCount * 2,
                elasticWorkersTimeout: TimeSpan.FromMilliseconds(100));
            _pool = HybridPool.Create(options);
        }

        [Benchmark(Description = "Memory: 10000 tasks - GC Analysis")]
        public async Task Memory_10000_Tasks_GCAnalysis()
        {
            for (int i = 0; i < 10000; i++)
            {
                var value = i;
                _pool.Enqueue(async () => 
                {
                    await Task.Delay(1);
                    var result = value % 100;
                }, weight: i % 19);
            }

            await WaitForCompletion(_pool);
        }

        [Benchmark(Description = "Memory: 5000 sync tasks - Zero Allocations")]
        public void Memory_5000_Sync_ZeroAllocations()
        {
            for (int i = 0; i < 5000; i++)
            {
                var value = i;
                _pool.Enqueue(() => 
                {
                    var result = value % 1000;
                }, weight: i % 23);
            }
        }

        private async Task WaitForCompletion(HybridPool pool)
        {
            var timeout = TimeSpan.FromSeconds(30);
            var start = DateTime.UtcNow;
            
            while (pool.PendingWorkCount() > 0 && DateTime.UtcNow - start < timeout)
            {
                await Task.Delay(5);
            }
        }

        public void Dispose()
        {
            _pool?.Dispose();
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<HybridPoolBenchmarks>();
            
            Console.WriteLine("\n=== MEMORY ANALYSIS ===");
            BenchmarkRunner.Run<MemoryAnalysisBenchmarks>();
        }
    }
}
