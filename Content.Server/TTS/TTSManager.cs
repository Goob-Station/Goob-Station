using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Content.Shared.CCVar;
using Prometheus;
using Robust.Shared.Configuration;
using Robust.Shared.ContentPack;
using Robust.Shared.Utility;

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
    private readonly HttpClient _httpClient = new();
    private ISawmill _sawmill = default!;

    private string _modelPath = "";
    private readonly Dictionary<string, byte[]> _cache = new();
    private readonly List<string> _cacheKeysSeq = new();
    private int _maxCachedCount = 256;
    private string _apiUrl = string.Empty;
    private string _apiToken = string.Empty;

    public TTSManager()
    {
        Initialize();
    }

    public void Initialize()
    {
        IoCManager.InjectDependencies(this);
        _sawmill = Logger.GetSawmill("tts");

        _sawmill.Log(LogLevel.Info, _modelPath);
        UpdateModelPath(_cfg.GetCVar(CCVars.TTSModelPath));
        _cfg.OnValueChanged(CCVars.TTSModelPath, UpdateModelPath);

        // _cfg.OnValueChanged(CCCVars.TTSMaxCache, val =>
        // {
        //     _maxCachedCount = val;
        //     ResetCache();
        // }, true);
        // _cfg.OnValueChanged(CCCVars.TTSApiUrl, v => _apiUrl = v, true);
        // _cfg.OnValueChanged(CCCVars.TTSApiToken, v => _apiToken = v, true);

        return;

        void UpdateModelPath(string path)
        {
            if (string.IsNullOrEmpty(path) && _cfg.GetCVar(CCVars.TTSEnabled))
            {
                DebugTools.Assert(false, "CVar TTSModelPath is unset but TTS is enabled.");
                return;
            }

            if (path.StartsWith("data/"))
                _modelPath = _resource.UserData.RootDir + path.Remove(0, 5);
            else
                _modelPath = path; // Hope it's valid

            _sawmill.Log(LogLevel.Info, _modelPath);
        }
    }

    /// <summary>
    /// Generates audio with passed text by API
    /// </summary>
    /// <param name="speaker">Identifier of speaker</param>
    /// <param name="text">SSML formatted text</param>
    /// <returns>OGG audio bytes or null if failed</returns>
    public async Task<byte[]?> ConvertTextToSpeech(string model, string speaker, string text)
    {
        var fileName = text.GetHashCode() + ".wav";
        var strCmdText = $"echo '{text}' | piper --model {_modelPath}/{model}.onnx --output_file {fileName} --speaker {speaker}";

        var proc = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/sh",
                Arguments = $"-c \"{strCmdText}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            }
        };

        if (_cfg.GetCVar(CCVars.HostWindows))
        {
            strCmdText = $"echo '{text}' | piper --model {_modelPath}\\{model}.onnx --output_file {fileName} --speaker {speaker}";

            proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C \"{strCmdText}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                }
            };
        }


        var reqTime = DateTime.UtcNow;
        try
        {
            proc.Start();
            await proc.WaitForExitAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            RequestTimings.WithLabels("Error").Observe((DateTime.UtcNow - reqTime).TotalSeconds);
            _sawmill.Error($"Failed of request generation new sound for '{text}' speech by '{speaker}' speaker\n{e}");
            return null;
        }

        // proc.WaitForExit();

        byte[] file = File.ReadAllBytes(fileName);
        return file;

        WantedCount.Inc();
        var cacheKey = GenerateCacheKey(speaker, text);
        if (_cache.TryGetValue(cacheKey, out var data))
        {
            ReusedCount.Inc();
            _sawmill.Verbose($"Use cached sound for '{text}' speech by '{speaker}' speaker");
            return data;
        }

        _sawmill.Verbose($"Generate new audio for '{text}' speech by '{speaker}' speaker");

        var body = new GenerateVoiceRequest
        {
            ApiToken = _apiToken,
            Text = text,
            Speaker = speaker,
        };

        reqTime = DateTime.UtcNow;
        try
        {
            var timeout = 0.25;
            // var timeout = _cfg.GetCVar(CCCVars.TTSApiTimeout);
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeout));
            var response = await _httpClient.PostAsJsonAsync(_apiUrl, body, cts.Token);
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    _sawmill.Warning("TTS request was rate limited");
                    return null;
                }

                _sawmill.Error($"TTS request returned bad status code: {response.StatusCode}");
                return null;
            }

            var json = await response.Content.ReadFromJsonAsync<GenerateVoiceResponse>(cancellationToken: cts.Token);
            var soundData = Convert.FromBase64String(json.Results.First().Audio);

            _cache.Add(cacheKey, soundData);
            _cacheKeysSeq.Add(cacheKey);
            if (_cache.Count > _maxCachedCount)
            {
                var firstKey = _cacheKeysSeq.First();
                _cache.Remove(firstKey);
                _cacheKeysSeq.Remove(firstKey);
            }

            _sawmill.Debug($"Generated new audio for '{text}' speech by '{speaker}' speaker ({soundData.Length} bytes)");
            RequestTimings.WithLabels("Success").Observe((DateTime.UtcNow - reqTime).TotalSeconds);

            return soundData;
        }
        catch (TaskCanceledException)
        {
            RequestTimings.WithLabels("Timeout").Observe((DateTime.UtcNow - reqTime).TotalSeconds);
            _sawmill.Error($"Timeout of request generation new audio for '{text}' speech by '{speaker}' speaker");
            return null;
        }
        catch (Exception e)
        {
            RequestTimings.WithLabels("Error").Observe((DateTime.UtcNow - reqTime).TotalSeconds);
            _sawmill.Error($"Failed of request generation new sound for '{text}' speech by '{speaker}' speaker\n{e}");
            return null;
        }
    }

    public void ResetCache()
    {
        _cache.Clear();
        _cacheKeysSeq.Clear();
    }

    private string GenerateCacheKey(string speaker, string text)
    {
        var key = $"{speaker}/{text}";
        byte[] keyData = Encoding.UTF8.GetBytes(key);
        var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = sha256.ComputeHash(keyData);
        return Convert.ToHexString(bytes);
    }

    private struct GenerateVoiceRequest
    {
        public GenerateVoiceRequest()
        {
        }

        [JsonPropertyName("api_token")]
        public string ApiToken { get; set; } = "";

        [JsonPropertyName("text")]
        public string Text { get; set; } = "";

        [JsonPropertyName("speaker")]
        public string Speaker { get; set; } = "";

        [JsonPropertyName("ssml")]
        public bool SSML { get; private set; } = true;

        [JsonPropertyName("word_ts")]
        public bool WordTS { get; private set; } = false;

        [JsonPropertyName("put_accent")]
        public bool PutAccent { get; private set; } = true;

        [JsonPropertyName("put_yo")]
        public bool PutYo { get; private set; } = false;

        [JsonPropertyName("sample_rate")]
        public int SampleRate { get; private set; } = 24000;

        [JsonPropertyName("format")]
        public string Format { get; private set; } = "ogg";
    }

    private struct GenerateVoiceResponse
    {
        [JsonPropertyName("results")]
        public List<VoiceResult> Results { get; set; }

        [JsonPropertyName("original_sha1")]
        public string Hash { get; set; }
    }

    private struct VoiceResult
    {
        [JsonPropertyName("audio")]
        public string Audio { get; set; }
    }
}
