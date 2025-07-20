using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.GC;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace JapaneseTracker.Services
{
    /// <summary>
    /// Performance monitoring service for Phase 5 optimization requirements
    /// Monitors application performance, memory usage, and database query performance
    /// </summary>
    public class PerformanceMonitoringService
    {
        private readonly ILogger<PerformanceMonitoringService> _logger;
        private readonly Dictionary<string, PerformanceMetric> _metrics;
        private readonly Stopwatch _applicationStopwatch;

        public PerformanceMonitoringService(ILogger<PerformanceMonitoringService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _metrics = new Dictionary<string, PerformanceMetric>();
            _applicationStopwatch = Stopwatch.StartNew();
            
            // Start monitoring on service initialization
            StartMonitoring();
        }

        #region Performance Metrics

        public class PerformanceMetric
        {
            public string Name { get; set; } = string.Empty;
            public TimeSpan TotalTime { get; set; }
            public int CallCount { get; set; }
            public TimeSpan AverageTime => CallCount > 0 ? TimeSpan.FromTicks(TotalTime.Ticks / CallCount) : TimeSpan.Zero;
            public TimeSpan MinTime { get; set; } = TimeSpan.MaxValue;
            public TimeSpan MaxTime { get; set; } = TimeSpan.MinValue;
            public DateTime LastCalled { get; set; }
            public List<Exception> Errors { get; set; } = new List<Exception>();
        }

        #endregion

        #region Monitoring Operations

        /// <summary>
        /// Start monitoring application performance
        /// </summary>
        private void StartMonitoring()
        {
            _logger.LogInformation("Performance monitoring started");
            
            // Log initial system state
            LogSystemResources();
        }

        /// <summary>
        /// Measure execution time of an operation
        /// </summary>
        public async Task<T> MeasureAsync<T>(string operationName, Func<Task<T>> operation)
        {
            var stopwatch = Stopwatch.StartNew();
            var startMemory = GC.GetTotalMemory(false);
            
            try
            {
                _logger.LogDebug("Starting operation: {OperationName}", operationName);
                
                var result = await operation();
                
                stopwatch.Stop();
                var endMemory = GC.GetTotalMemory(false);
                var memoryUsed = endMemory - startMemory;
                
                RecordMetric(operationName, stopwatch.Elapsed, null);
                
                _logger.LogDebug("Completed operation: {OperationName} in {ElapsedTime}ms, Memory: {MemoryUsed} bytes",
                    operationName, stopwatch.ElapsedMilliseconds, memoryUsed);
                
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                RecordMetric(operationName, stopwatch.Elapsed, ex);
                
                _logger.LogError(ex, "Operation failed: {OperationName} after {ElapsedTime}ms",
                    operationName, stopwatch.ElapsedMilliseconds);
                
                throw;
            }
        }

        /// <summary>
        /// Measure execution time of a synchronous operation
        /// </summary>
        public T Measure<T>(string operationName, Func<T> operation)
        {
            var stopwatch = Stopwatch.StartNew();
            var startMemory = GC.GetTotalMemory(false);
            
            try
            {
                _logger.LogDebug("Starting operation: {OperationName}", operationName);
                
                var result = operation();
                
                stopwatch.Stop();
                var endMemory = GC.GetTotalMemory(false);
                var memoryUsed = endMemory - startMemory;
                
                RecordMetric(operationName, stopwatch.Elapsed, null);
                
                _logger.LogDebug("Completed operation: {OperationName} in {ElapsedTime}ms, Memory: {MemoryUsed} bytes",
                    operationName, stopwatch.ElapsedMilliseconds, memoryUsed);
                
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                RecordMetric(operationName, stopwatch.Elapsed, ex);
                
                _logger.LogError(ex, "Operation failed: {OperationName} after {ElapsedTime}ms",
                    operationName, stopwatch.ElapsedMilliseconds);
                
                throw;
            }
        }

        /// <summary>
        /// Record a performance metric
        /// </summary>
        private void RecordMetric(string operationName, TimeSpan elapsed, Exception? error)
        {
            lock (_metrics)
            {
                if (!_metrics.TryGetValue(operationName, out var metric))
                {
                    metric = new PerformanceMetric { Name = operationName };
                    _metrics[operationName] = metric;
                }

                metric.TotalTime = metric.TotalTime.Add(elapsed);
                metric.CallCount++;
                metric.LastCalled = DateTime.UtcNow;

                if (elapsed < metric.MinTime)
                    metric.MinTime = elapsed;
                if (elapsed > metric.MaxTime)
                    metric.MaxTime = elapsed;

                if (error != null)
                {
                    metric.Errors.Add(error);
                }
            }
        }

        #endregion

        #region Performance Analysis

        /// <summary>
        /// Get performance report for all monitored operations
        /// </summary>
        public Dictionary<string, PerformanceMetric> GetPerformanceReport()
        {
            lock (_metrics)
            {
                return new Dictionary<string, PerformanceMetric>(_metrics);
            }
        }

        /// <summary>
        /// Get performance statistics for a specific operation
        /// </summary>
        public PerformanceMetric? GetOperationMetrics(string operationName)
        {
            lock (_metrics)
            {
                return _metrics.TryGetValue(operationName, out var metric) ? metric : null;
            }
        }

        /// <summary>
        /// Log current system resource usage
        /// </summary>
        public void LogSystemResources()
        {
            var process = Process.GetCurrentProcess();
            var gcInfo = GC.GetGCMemoryInfo();
            
            _logger.LogInformation("System Resources - " +
                "Working Set: {WorkingSet} MB, " +
                "Private Memory: {PrivateMemory} MB, " +
                "GC Memory: {GCMemory} MB, " +
                "GC Gen0: {Gen0}, Gen1: {Gen1}, Gen2: {Gen2}",
                process.WorkingSet64 / 1024 / 1024,
                process.PrivateMemorySize64 / 1024 / 1024,
                GC.GetTotalMemory(false) / 1024 / 1024,
                GC.CollectionCount(0),
                GC.CollectionCount(1),
                GC.CollectionCount(2));
        }

        /// <summary>
        /// Identify slow operations that may need optimization
        /// </summary>
        public List<string> IdentifySlowOperations(TimeSpan threshold)
        {
            var slowOperations = new List<string>();
            
            lock (_metrics)
            {
                foreach (var kvp in _metrics)
                {
                    if (kvp.Value.AverageTime > threshold || kvp.Value.MaxTime > threshold.Add(threshold))
                    {
                        slowOperations.Add($"{kvp.Key}: Avg={kvp.Value.AverageTime.TotalMilliseconds:F2}ms, " +
                                          $"Max={kvp.Value.MaxTime.TotalMilliseconds:F2}ms, " +
                                          $"Calls={kvp.Value.CallCount}");
                    }
                }
            }
            
            return slowOperations;
        }

        /// <summary>
        /// Suggest performance optimizations based on metrics
        /// </summary>
        public List<string> GetOptimizationSuggestions()
        {
            var suggestions = new List<string>();
            var slowOps = IdentifySlowOperations(TimeSpan.FromMilliseconds(100));
            
            if (slowOps.Count > 0)
            {
                suggestions.Add($"Found {slowOps.Count} operations that may benefit from optimization:");
                suggestions.AddRange(slowOps);
            }

            var gcInfo = GC.GetGCMemoryInfo();
            if (gcInfo.HighMemoryLoadThresholdBytes > 0 && 
                GC.GetTotalMemory(false) > gcInfo.HighMemoryLoadThresholdBytes * 0.8)
            {
                suggestions.Add("Memory usage is high. Consider optimizing data structures or implementing caching strategies.");
            }

            if (GC.CollectionCount(2) > 10)
            {
                suggestions.Add("High number of Gen2 garbage collections detected. Review object lifetime management.");
            }

            return suggestions;
        }

        #endregion

        #region Database Query Performance

        /// <summary>
        /// Monitor database query performance with specific thresholds
        /// </summary>
        public async Task<T> MonitorDatabaseQueryAsync<T>(string queryName, Func<Task<T>> query)
        {
            // Database queries should complete quickly for good UX
            var threshold = TimeSpan.FromMilliseconds(50);
            
            var result = await MeasureAsync($"DB_{queryName}", query);
            
            var metric = GetOperationMetrics($"DB_{queryName}");
            if (metric != null && metric.AverageTime > threshold)
            {
                _logger.LogWarning("Slow database query detected: {QueryName} - Average: {AverageTime}ms, " +
                                  "consider adding indexes or optimizing query",
                                  queryName, metric.AverageTime.TotalMilliseconds);
            }
            
            return result;
        }

        #endregion

        #region Memory Management

        /// <summary>
        /// Force garbage collection and log memory statistics
        /// </summary>
        public void ForceGarbageCollection()
        {
            var beforeMemory = GC.GetTotalMemory(false);
            
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            var afterMemory = GC.GetTotalMemory(true);
            var freedMemory = beforeMemory - afterMemory;
            
            _logger.LogInformation("Garbage collection completed. " +
                "Memory before: {BeforeMemory} MB, " +
                "Memory after: {AfterMemory} MB, " +
                "Freed: {FreedMemory} MB",
                beforeMemory / 1024 / 1024,
                afterMemory / 1024 / 1024,
                freedMemory / 1024 / 1024);
        }

        #endregion

        #region Reporting

        /// <summary>
        /// Generate a comprehensive performance report
        /// </summary>
        public string GeneratePerformanceReport()
        {
            var report = new System.Text.StringBuilder();
            var uptime = _applicationStopwatch.Elapsed;
            
            report.AppendLine("=== Japanese Learning Tracker Performance Report ===");
            report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"Uptime: {uptime.Days}d {uptime.Hours:D2}h {uptime.Minutes:D2}m {uptime.Seconds:D2}s");
            report.AppendLine();
            
            // System resources
            report.AppendLine("=== System Resources ===");
            var process = Process.GetCurrentProcess();
            report.AppendLine($"Working Set: {process.WorkingSet64 / 1024 / 1024} MB");
            report.AppendLine($"Private Memory: {process.PrivateMemorySize64 / 1024 / 1024} MB");
            report.AppendLine($"GC Memory: {GC.GetTotalMemory(false) / 1024 / 1024} MB");
            report.AppendLine($"GC Collections - Gen0: {GC.CollectionCount(0)}, Gen1: {GC.CollectionCount(1)}, Gen2: {GC.CollectionCount(2)}");
            report.AppendLine();
            
            // Performance metrics
            report.AppendLine("=== Operation Performance ===");
            lock (_metrics)
            {
                foreach (var kvp in _metrics)
                {
                    var metric = kvp.Value;
                    report.AppendLine($"{metric.Name}:");
                    report.AppendLine($"  Calls: {metric.CallCount}");
                    report.AppendLine($"  Total: {metric.TotalTime.TotalMilliseconds:F2}ms");
                    report.AppendLine($"  Average: {metric.AverageTime.TotalMilliseconds:F2}ms");
                    report.AppendLine($"  Min: {metric.MinTime.TotalMilliseconds:F2}ms");
                    report.AppendLine($"  Max: {metric.MaxTime.TotalMilliseconds:F2}ms");
                    report.AppendLine($"  Errors: {metric.Errors.Count}");
                    report.AppendLine($"  Last Called: {metric.LastCalled:yyyy-MM-dd HH:mm:ss}");
                    report.AppendLine();
                }
            }
            
            // Optimization suggestions
            var suggestions = GetOptimizationSuggestions();
            if (suggestions.Count > 0)
            {
                report.AppendLine("=== Optimization Suggestions ===");
                foreach (var suggestion in suggestions)
                {
                    report.AppendLine($"• {suggestion}");
                }
                report.AppendLine();
            }
            
            return report.ToString();
        }

        #endregion
    }
}