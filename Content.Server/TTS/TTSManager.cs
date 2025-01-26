using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Content.Shared._Goobstation.CCVar;
using Prometheus;
using Robust.Shared.Configuration;
using Robust.Shared.ContentPack;
using Robust.Shared.Utility;
using System.Threading;

namespace Content.Server.TTS;

// ReSharper disable once InconsistentNaming
public sealed class TTSManager
{
    private static readonly Histogram RequestTimings = Metrics.CreateHistogram(
        "tts_req_timings",
        "Timings of TTS API requests",
        new HistogramConfiguration()
        {
            LabelNames = new[] { "type" },
            Buckets = Histogram.ExponentialBuckets(.1, 1.5, 10),
        });

    private static readonly Counter WantedCount = Metrics.CreateCounter(
        "tts_wanted_count",
        "Amount of wanted TTS audio.");

    private static readonly Counter ReusedCount = Metrics.CreateCounter(
        "tts_reused_count",
        "Amount of reused TTS audio from cache.");

    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IResourceManager _resource = default!;
    private ISawmill _sawmill = default!;

    private readonly Dictionary<int, byte[]> _memoryCache = new();
    private ResPath _cachePath = new();
    private ResPath _modelPath = new();

    private SemaphoreSlim _generationSemaphore = new SemaphoreSlim(1);
    private int _queuedGenerations = 0;
    private int _maxQueuedGenerations = 20;

    public TTSManager()
    {
        Initialize();
    }

    private void Initialize()
    {
        IoCManager.InjectDependencies(this);
        _sawmill = Logger.GetSawmill("tts");

        _cachePath = MakeDataPath(_cfg.GetCVar(GoobCVars.TTSCachePath));
        _cfg.OnValueChanged(GoobCVars.TTSCachePath, OnCachePathChanged);
        _modelPath = MakeDataPath(_cfg.GetCVar(GoobCVars.TTSModelPath));
        _cfg.OnValueChanged(GoobCVars.TTSModelPath, OnModelPathChanged);
        _generationSemaphore = new SemaphoreSlim(_cfg.GetCVar(GoobCVars.TTSSimultaneousGenerations));
        _cfg.OnValueChanged(GoobCVars.TTSSimultaneousGenerations, OnRateLimitChanged);
        _maxQueuedGenerations = _cfg.GetCVar(GoobCVars.TTSQueueMax);
        _cfg.OnValueChanged(GoobCVars.TTSQueueMax, OnQueueMaxChanged);

        // Make the needed directories if they don't exist
        new Process
        {
            StartInfo = new ProcessStartInfo
            {
                #if WINDOWS
                FileName = "cmd.exe",
                Arguments = $"/C \"mkdir {_cachePath} {_modelPath}\"",
                #else
                FileName = "/bin/sh",
                Arguments = $"-c \"mkdir -p {_cachePath} {_modelPath}\"",
                #endif
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            },
        }.Start();
    }

    private void OnCachePathChanged(string path)
        => _cachePath = MakeDataPath(path);
    private void OnModelPathChanged(string path)
        => _modelPath = MakeDataPath(path);
    private void OnRateLimitChanged(int limit)
    {
        int currentCount = _generationSemaphore.CurrentCount;

        if (limit > currentCount)
            _generationSemaphore.Release(limit - currentCount);
        else if (limit < currentCount)
        {
            for (int i = 0; i < (currentCount - limit); i++)
                _generationSemaphore.Wait();
        }
    }
    private void OnQueueMaxChanged(int maxQueue)
        => _maxQueuedGenerations = maxQueue;

    private ResPath MakeDataPath(string path)
    {
        if (path.StartsWith("data/"))
            return new(_resource.UserData.RootDir + path.Remove(0, 5));
        else
            return new(path); // Hope it's valid
    }


    /// <summary>
    /// Generates audio with passed text by API
    /// </summary>
    /// <param name="model">File name for the model</param>
    /// <param name="speaker">Identifier of speaker</param>
    /// <param name="text">SSML formatted text</param>
    /// <returns>OGG audio bytes or null if failed</returns>
    public async Task<byte[]?> ConvertTextToSpeech(string model, string speaker, string text)
    {
        WantedCount.Inc();

        var key = $"{model}/{speaker}/{text}".GetHashCode();
        var cachedData = await TryGetCached(key);
        if (cachedData != null)
        {
            ReusedCount.Inc();
            return cachedData;
        }

        // TODO:
        // Instead of just incrementing a integer, we should really keep track of what text + voice is in queue to be generated
        // This would stop the issue of Urist McHands saying "godo" 30 times before the first "godo" can even be generated and added to the cache
        // Which would cause it to try to generate the same message 30 times, and would instead just waiting for the first one to generate and then
        // just reuse the cached version of it.

        if (Interlocked.Increment(ref _queuedGenerations) > _maxQueuedGenerations)
        {
            Interlocked.Decrement(ref _queuedGenerations);
            _sawmill.Warning($"Queue limit exceeded for TTS generation: {text}");
            return null;
        }

        try
        {
            await _generationSemaphore.WaitAsync();

            try
            {
                var strCmdText = $"echo '{text}' | piper --model {(_modelPath + ResPath.SystemSeparatorStr + model)}.onnx --speaker {speaker} --output_raw";

                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        #if WINDOWS
                        FileName = "cmd.exe",
                        Arguments = $"/C \"{strCmdText}\"",
                        #else
                        FileName = "/bin/sh",
                        Arguments = $"-c \"{strCmdText}\"",
                        #endif
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true,
                    },
                };

                var reqTime = DateTime.UtcNow;
                try
                {
                    proc.Start();

                    using var memoryStream = new MemoryStream();
                    await proc.StandardOutput.BaseStream.CopyToAsync(memoryStream).ConfigureAwait(false);

                    await proc.WaitForExitAsync().ConfigureAwait(false);

                    if (proc.ExitCode != 0)
                    {
                        RequestTimings.WithLabels("Error").Observe((DateTime.UtcNow - reqTime).TotalSeconds);
                        _sawmill.Error($"Piper process failed for '{text}' speech by '{speaker}' speaker.");
                        return null;
                    }

                    var rawData = memoryStream.ToArray();
                    TryCache(key, rawData);
                    return rawData;
                }
                catch (Exception e)
                {
                    RequestTimings.WithLabels("Error").Observe((DateTime.UtcNow - reqTime).TotalSeconds);
                    _sawmill.Error($"Failed to generate new sound for '{text}' speech by '{speaker}' speaker\n{e}");
                    return null;
                }
            }
            finally
            {
                _generationSemaphore.Release();
            }
        }
        finally
        {
            Interlocked.Decrement(ref _queuedGenerations);
        }
    }

    private bool TryCache(int key, byte[] file)
    {
        if (_cfg.GetCVar(GoobCVars.TTSCacheType) != "memory")
        {
            var files = Directory.GetFiles(_cachePath + ResPath.SystemSeparatorStr).ToList()
                .OrderBy(f => File.GetLastWriteTimeUtc(f).Ticks);
            var count = files.Count();
            var toDelete = count - _cfg.GetCVar(GoobCVars.TTSMaxCached);

            for (var i = toDelete; i > 0; i--)
            {
                File.Delete(files.ElementAt(i));
            }

            var filePath = _cachePath + ResPath.SystemSeparatorStr + key + ".raw";
            File.WriteAllBytes(filePath, file);

            return true;
        }

        // Handle memory caching
        while (_memoryCache.Count > _cfg.GetCVar(GoobCVars.TTSMaxCached))
        {
            _memoryCache.Remove(_memoryCache.First().Key);
        }

        // Cache to memory
        return _memoryCache.TryAdd(key, file);
    }


    /// Tries to find an existing audio file so we don't have to make another
    private async Task<byte[]?> TryGetCached(int key)
    {
        var type = _cfg.GetCVar(GoobCVars.TTSCacheType);
        switch (type)
        {
            case "file":
                var path = _cachePath + ResPath.SystemSeparatorStr + key + ".raw";
                return !File.Exists(path) ? null : await File.ReadAllBytesAsync(path);
            case "memory":
                return _memoryCache.GetValueOrDefault(key);
            default:
                DebugTools.Assert(false, "TTSCacheType is invalid, must be one of \"file\", \"memory\"");
                return null;
        }
    }

    /// Deletes every file with the .raw extension in the _cachePath and clears the memory cache
    public void ClearCache()
    {
        new Process
        {
            StartInfo = new ProcessStartInfo
            {
                #if WINDOWS
                FileName = "cmd.exe",
                Arguments = $"/C \"del /q {_cachePath}\\*.raw\"",
                #else
                FileName = "/bin/sh",
                Arguments = $"-c \"rm {_cachePath}/*.raw\"",
                #endif
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            },
        }.Start();
        _memoryCache.Clear();
    }
}
