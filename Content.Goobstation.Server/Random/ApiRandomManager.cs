using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Prometheus;
using Robust.Shared.IoC;
using Robust.Shared.Log;

namespace Content.Goobstation.Server.Random;

/// <summary>
///     Manages fetching and pooling random numbers from the NousRandom API.
/// </summary>
public sealed class ApiRandomManager
{
    private readonly ConcurrentQueue<int> _intPool = new();
    private readonly ConcurrentQueue<float> _floatPool = new();

    private readonly HttpClient _httpClient = new();
    private ISawmill _sawmill = default!;

    private const int TargetPoolSize = 10000;
    private const int EmergencyRefillThreshold = 1000;

    // I LOVE METRICS!!!!
    private static readonly Counter IntsUsed = Metrics.CreateCounter("goob_random_ints_used", "Number of integers used from the random pool.");
    private static readonly Counter FloatsUsed = Metrics.CreateCounter("goob_random_floats_used", "Number of floats used from the random pool.");
    private static readonly Counter ApiRequests = Metrics.CreateCounter("goob_random_api_requests", "Number of API requests made to NousRandom.");
    private static readonly Gauge IntPoolSize = Metrics.CreateGauge("goob_random_int_pool_size", "Current size of the integer pool.");
    private static readonly Gauge FloatPoolSize = Metrics.CreateGauge("goob_random_float_pool_size", "Current size of the float pool.");
    private static readonly Counter Fallbacks = Metrics.CreateCounter("goob_random_fallbacks", "Number of times the system fell back to System.Random.");

    public void Initialize()
    {
        _sawmill = Logger.GetSawmill("goob.random");
        Task.Run(ScheduledRefill, CancellationToken.None);
    }

    public void Shutdown()
    {
        _httpClient.Dispose();
    }

    private async Task ScheduledRefill()
    {
        while (true)
        {
            await Task.Delay(10000);
            await RefillPools();
        }
    }

    private async Task RefillPools()
    {
        await RefillIntPool();
        await RefillFloatPool();
    }

    private async Task RefillIntPool()
    {
        var needed = TargetPoolSize - _intPool.Count;
        if (needed > 0)
        {
            await FetchIntegers(needed);
        }
    }

    private async Task RefillFloatPool()
    {
        var needed = TargetPoolSize - _floatPool.Count;
        if (needed > 0)
        {
            await FetchFloats(needed);
        }
    }

    private async Task FetchIntegers(int count)
    {
        try
        {
            ApiRequests.Inc();
            var response = await _httpClient.GetStringAsync($"https://nousrandom.net/api/v1/?int;min=0;max=2147483647;cnt={count};json;nohtml");
            var json = JsonDocument.Parse(response);
            var result = json.RootElement.GetProperty("result");

            foreach (var number in result.EnumerateArray())
            {
                if (number.TryGetInt32(out var intValue))
                    _intPool.Enqueue(intValue);
            }
        }
        catch (Exception e)
        {
            _sawmill.Error($"Failed to fetch random integers: {e.Message}");
        }
        finally
        {
            IntPoolSize.Set(_intPool.Count);
        }
    }

    private async Task FetchFloats(int count)
    {
        try
        {
            ApiRequests.Inc();
            var response = await _httpClient.GetStringAsync($"https://nousrandom.net/api/v1/?flt;min=-1.0;max=1.0;cnt={count};json;nohtml");
            var json = JsonDocument.Parse(response);
            var result = json.RootElement.GetProperty("result");

            foreach (var number in result.EnumerateArray())
            {
                if (number.TryGetSingle(out var floatValue))
                    _floatPool.Enqueue(floatValue);
            }
        }
        catch (Exception e)
        {
            _sawmill.Error($"Failed to fetch random floats: {e.Message}");
        }
        finally
        {
            FloatPoolSize.Set(_floatPool.Count);
        }
    }

    public bool TryGetInt(out int value)
    {
        if (_intPool.Count < EmergencyRefillThreshold)
            Task.Run(RefillIntPool, CancellationToken.None);

        if (_intPool.TryDequeue(out value))
        {
            IntsUsed.Inc();
            IntPoolSize.Dec();
            return true;
        }

        Fallbacks.Inc();
        value = 0;
        return false;
    }

    public bool TryGetFloat(out float value)
    {
        if (_floatPool.Count < EmergencyRefillThreshold)
            Task.Run(RefillFloatPool, CancellationToken.None);

        if (_floatPool.TryDequeue(out value))
        {
            FloatsUsed.Inc();
            FloatPoolSize.Dec();
            return true;
        }

        Fallbacks.Inc();
        value = 0f;
        return false;
    }
}
