// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Client.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Timing;
using Robust.Shared.Configuration;
using Content.Goobstation.Common.CCVar;
using System.Linq;

namespace Content.Goobstation.Client.Voice;

public sealed class VoiceStreamManager : IDisposable
{
    private readonly Queue<byte[]> _packetQueue = new();
    private readonly object _queueLock = new();
    private bool _isPlaying;
    private bool _isDisposed;
    private TimeSpan? _expectedChunkEndTime;

    private readonly int _sampleRate;
    private readonly IAudioManager _audioManager;
    private readonly AudioSystem _audioSystem;
    private readonly EntityUid _sourceEntity;
    private readonly ISawmill _sawmill;
    private readonly IGameTiming _gameTiming;

    // New adaptive components
    private readonly NetworkConditionMonitor _networkMonitor;
    private readonly AdaptiveBufferThresholds _bufferThresholds;
    private readonly AdaptivePlaybackEngine _playbackEngine;
    private readonly IConfigurationManager _cfg;

    private const int BytesPerSample = 2;
    private const int Channels = 1;
    private int _maxQueuedPackets = 50;
    private float _volume = 0.5f;
    private readonly TimeSpan _chunkOverlap = TimeSpan.FromMilliseconds(0);

    public VoiceStreamManager(IAudioManager audioManager, AudioSystem audioSystem,
                             EntityUid sourceEntity, int sampleRate)
    {
        _audioManager = audioManager;
        _audioSystem = audioSystem;
        _sourceEntity = sourceEntity;
        _sampleRate = sampleRate;
        _sawmill = Logger.GetSawmill("voiceclient");
        _gameTiming = IoCManager.Resolve<IGameTiming>();
        _cfg = IoCManager.Resolve<IConfigurationManager>();

        var bufferMultiplier = _cfg.GetCVar(GoobCVars.VoiceChatBufferTargetMultiplier);
        var minBufferSize = _cfg.GetCVar(GoobCVars.VoiceChatMinBufferSize);
        var maxBufferSize = _cfg.GetCVar(GoobCVars.VoiceChatMaxBufferSize);
        var debugLogging = _cfg.GetCVar(GoobCVars.VoiceChatDebugLogging);
        _volume = _cfg.GetCVar(GoobCVars.VoiceChatVolume);
        _sawmill.Info($"VoiceStreamManager initialized for entity {_sourceEntity} with volume: {_volume}");

        _maxQueuedPackets = maxBufferSize;

        _networkMonitor = new NetworkConditionMonitor(bufferMultiplier, minBufferSize, maxBufferSize);
        _bufferThresholds = new AdaptiveBufferThresholds(_networkMonitor);
        _playbackEngine = new AdaptivePlaybackEngine(_bufferThresholds, _sawmill, debugLogging);

        _cfg.OnValueChanged(GoobCVars.VoiceChatBufferTargetMultiplier, OnBufferMultiplierChanged);
        _cfg.OnValueChanged(GoobCVars.VoiceChatMinBufferSize, OnMinBufferSizeChanged);
        _cfg.OnValueChanged(GoobCVars.VoiceChatMaxBufferSize, OnMaxBufferSizeChanged);
        _cfg.OnValueChanged(GoobCVars.VoiceChatDebugLogging, OnDebugLoggingChanged);
        _cfg.OnValueChanged(GoobCVars.VoiceChatVolume, OnVolumeChanged);
    }

    /// <summary>
    /// Handle changes to buffer target multiplier CVar.
    /// </summary>
    private void OnBufferMultiplierChanged(float newValue)
    {
        var minBufferSize = _cfg.GetCVar(GoobCVars.VoiceChatMinBufferSize);
        var maxBufferSize = _cfg.GetCVar(GoobCVars.VoiceChatMaxBufferSize);
        _networkMonitor.UpdateSettings(newValue, minBufferSize, maxBufferSize);
        _sawmill.Info($"[VOICE CVAR] Buffer multiplier updated to {newValue} for entity {_sourceEntity}");
    }

    /// <summary>
    /// Handle changes to minimum buffer size CVar.
    /// </summary>
    private void OnMinBufferSizeChanged(int newValue)
    {
        var bufferMultiplier = _cfg.GetCVar(GoobCVars.VoiceChatBufferTargetMultiplier);
        var maxBufferSize = _cfg.GetCVar(GoobCVars.VoiceChatMaxBufferSize);
        _networkMonitor.UpdateSettings(bufferMultiplier, newValue, maxBufferSize);
        _sawmill.Info($"[VOICE CVAR] Min buffer size updated to {newValue} for entity {_sourceEntity}");
    }

    /// <summary>
    /// Handle changes to maximum buffer size CVar.
    /// </summary>
    private void OnMaxBufferSizeChanged(int newValue)
    {
        var bufferMultiplier = _cfg.GetCVar(GoobCVars.VoiceChatBufferTargetMultiplier);
        var minBufferSize = _cfg.GetCVar(GoobCVars.VoiceChatMinBufferSize);
        _networkMonitor.UpdateSettings(bufferMultiplier, minBufferSize, newValue);
        _maxQueuedPackets = newValue; // Update the actual queue size limit
        _sawmill.Info($"[VOICE CVAR] Max buffer size updated to {newValue} for entity {_sourceEntity}");
    }

    /// <summary>
    /// Handle changes to debug logging CVar.
    /// </summary>
    private void OnDebugLoggingChanged(bool newValue)
    {
        _playbackEngine.UpdateDebugLogging(newValue);
        _sawmill.Info($"[VOICE CVAR] Debug logging updated to {newValue} for entity {_sourceEntity}");
    }

    /// <summary>
    /// Handle changes to volume CVar.
    /// </summary>
    private void OnVolumeChanged(float newValue)
    {
        _volume = newValue;
        _sawmill.Info($"[VOICE CVAR] Volume updated to {newValue} for entity {_sourceEntity}");
    }

    /// <summary>
    /// Adds a packet of PCM audio data to the playback queue.
    /// </summary>
    public void AddPacket(byte[] pcmData)
    {
        if (_isDisposed) return;

        lock (_queueLock)
        {
            _networkMonitor.RecordPacketArrival(_gameTiming.CurTime);
            _bufferThresholds.UpdateTarget();

            if (_packetQueue.Count >= _maxQueuedPackets)
            {
                _sawmill.Warning($"[{_gameTiming.CurTime.TotalSeconds:F3}] Voice buffer full for {_sourceEntity} (Queue: {_packetQueue.Count}/{_maxQueuedPackets}). Dropping packet ({pcmData.Length} bytes).");
                return;
            }

            var dataCopy = new byte[pcmData.Length];
            Array.Copy(pcmData, dataCopy, pcmData.Length);
            _packetQueue.Enqueue(dataCopy);

            var debugLogging = _cfg.GetCVar(GoobCVars.VoiceChatDebugLogging);
            if (debugLogging && _packetQueue.Count % 5 == 0)
            {
                var targetSize = _networkMonitor.CalculateTargetBufferSize();
                var avgInterval = _networkMonitor.AverageInterval.TotalMilliseconds;
                var jitter = _networkMonitor.Jitter.TotalMilliseconds;
                _sawmill.Info($"[VOICE BUFFER] Target: {targetSize}, Queue: {_packetQueue.Count}, AvgInterval: {avgInterval:F1}ms, Jitter: {jitter:F1}ms");
            }
            else
            {
                _sawmill.Debug($"[{_gameTiming.CurTime.TotalSeconds:F3}] AddPacket: Packet received for {_sourceEntity} ({pcmData.Length} bytes). Queue size now: {_packetQueue.Count}/{_maxQueuedPackets}");
            }

            if (!_isPlaying && _packetQueue.Count >= 2)
            {
                _sawmill.Debug($"[{_gameTiming.CurTime.TotalSeconds:F3}] Sufficient packets ({_packetQueue.Count}/2) for chunk reached for {_sourceEntity}. Starting playback.");
                _isPlaying = true;
                PlayNextChunk();
            }
        }
    }

    /// <summary>
    /// Processes multiple audio packets, either compressing, stretching, or concatenating them.
    /// </summary>
    /// <param name="packetsToProcess">List of packets (should contain PacketsPerChunk).</param>
    /// <param name="ratio">Compression (< 1.0) or Stretch (> 1.0) ratio. 1.0 for simple concatenation.</param>
    /// <returns>Processed audio data as byte array.</returns>
    private byte[] ProcessPackets(List<byte[]> packetsToProcess, float ratio)
    {
        int totalBytes = 0;
        foreach (var p in packetsToProcess) totalBytes += p.Length;

        if (Math.Abs(ratio - 1.0f) < 0.001f)
        {
            byte[] concatResult = new byte[totalBytes];
            int offset = 0;
            foreach (var packet in packetsToProcess)
            {
                for (int i = 0; i < packet.Length; i++) concatResult[offset + i] = packet[i];
                offset += packet.Length;
            }
            return concatResult;
        }

        var totalSamples = totalBytes / BytesPerSample;
        var targetSamples = (int) (totalSamples * ratio);
        byte[] result = new byte[targetSamples * BytesPerSample];
        int resultIndex = 0;

        for (int i = 0; i < targetSamples; i++)
        {
            var sourceSampleFloat = i / ratio;
            var sourceSampleIndex = (int) sourceSampleFloat;
            short sample1;
            short sample2;
            var fraction = sourceSampleFloat - sourceSampleIndex;

            if (!TryGetSample(packetsToProcess, sourceSampleIndex, out sample1))
            {
                _sawmill.Warning($"ProcessPackets calculation error: source index {sourceSampleIndex} out of bounds.");
                continue;
            }

            var finalSample = sample1;

            if (ratio > 1.0f && fraction > 0.001f && sourceSampleIndex + 1 < totalSamples)
                if (TryGetSample(packetsToProcess, sourceSampleIndex + 1, out sample2))
                    finalSample = (short) (sample1 + (sample2 - sample1) * fraction);

            if (resultIndex < result.Length - 1)
            {
                result[resultIndex++] = (byte) (finalSample & 0xFF);
                result[resultIndex++] = (byte) ((finalSample >> 8) & 0xFF);
            }
            else
            {
                _sawmill.Warning($"ProcessPackets calculation error: result index {resultIndex} out of bounds for result length {result.Length}");
                break;
            }
        }

        if (resultIndex != result.Length)
        {
            _sawmill.Warning($"ProcessPackets result size mismatch: expected {result.Length}, got {resultIndex}. Trimming.");
            Array.Resize(ref result, resultIndex);
        }
        return result;
    }

    /// <summary>
    /// Helper to get a specific 16-bit sample from a list of packet byte arrays.
    /// </summary>
    private bool TryGetSample(List<byte[]> packets, int globalSampleIndex, out short sample)
    {
        sample = 0;
        int globalByteIndex = globalSampleIndex * BytesPerSample;
        int bytesScanned = 0;

        foreach (var packet in packets)
        {
            if (globalByteIndex >= bytesScanned && globalByteIndex < bytesScanned + packet.Length - 1)
            {
                int indexInPacket = globalByteIndex - bytesScanned;
                byte b1 = packet[indexInPacket];
                byte b2 = packet[indexInPacket + 1];
                sample = (short) ((b2 << 8) | b1);
                return true;
            }
            bytesScanned += packet.Length;
        }
        return false;
    }


    /// <summary>
    /// Plays the next chunk of audio data using adaptive buffer management.
    /// </summary>
    private void PlayNextChunk()
    {
        if (_isDisposed) return;

        byte[]? pcmData = null;
        PlaybackDecision decision;

        lock (_queueLock)
        {
            int queueCount = _packetQueue.Count;

            decision = _playbackEngine.DecidePlaybackStrategy(queueCount);

            if (decision.PacketsToConsume == 0 || queueCount < decision.PacketsToConsume)
            {
                _sawmill.Debug($"[{_gameTiming.CurTime.TotalSeconds:F3}] PlayNextChunk: Not enough packets ({queueCount}/{decision.PacketsToConsume}) for {_sourceEntity}. Stopping playback.");
                _isPlaying = false;
                _expectedChunkEndTime = null;
                return;
            }

            _sawmill.Debug($"[{_gameTiming.CurTime.TotalSeconds:F3}] PlayNextChunk: Queue count: {queueCount}. Mode: {decision.Mode}");

            var packetsToProcess = new List<byte[]>(decision.PacketsToConsume);
            for (int i = 0; i < decision.PacketsToConsume; i++)
                packetsToProcess.Add(_packetQueue.Dequeue());

            pcmData = ProcessPackets(packetsToProcess, decision.TimeStretchRatio);

            _sawmill.Debug($"[{_gameTiming.CurTime.TotalSeconds:F3}] Decided Mode: {decision.Mode}. Processing (Ratio {decision.TimeStretchRatio:F2}) {decision.PacketsToConsume} packets for {_sourceEntity}. Queue size now: {_packetQueue.Count}/{_maxQueuedPackets}");
        }

        if (pcmData != null && pcmData.Length > 0)
        {
            var actualDuration = TimeSpan.FromSeconds((double) pcmData.Length / (_sampleRate * Channels * BytesPerSample));
            _sawmill.Debug($"[{_gameTiming.CurTime.TotalSeconds:F3}] Calculated Actual Duration: {actualDuration.TotalMilliseconds:F1}ms for {pcmData.Length} bytes.");
            try
            {
                short[] shortArray = ConvertToShortArray(pcmData);
                var audioStream = _audioManager.LoadAudioRaw(shortArray, Channels, _sampleRate);

                if (audioStream != null)
                {
                    var audioParams = AudioParams.Default
                        .WithVolume(_volume)
                        .WithMaxDistance(10f);

                    _sawmill.Info($"[VOICE AUDIO] Playing audio for entity {_sourceEntity} with volume {_volume} (bytes: {pcmData.Length}, duration: {actualDuration.TotalMilliseconds:F1}ms)");

                    var playResult = _sourceEntity.IsValid()
                        ? _audioSystem.PlayEntity(audioStream, _sourceEntity, null, audioParams)
                        : _audioSystem.PlayGlobal(audioStream, null, audioParams);

                    if (playResult != null)
                    {
                        _expectedChunkEndTime = _gameTiming.CurTime + actualDuration - _chunkOverlap;
                        _sawmill.Debug($"[{_gameTiming.CurTime.TotalSeconds:F3}] Playing chunk for {_sourceEntity}. Actual Duration: {actualDuration.TotalMilliseconds:F1}ms. Next check scheduled at: {_expectedChunkEndTime.Value.TotalSeconds:F3}");
                    }
                    else
                    {
                        _sawmill.Warning($"Failed to play audio for {_sourceEntity}. Attempting next chunk immediately.");
                        _expectedChunkEndTime = null;
                        PlayNextChunk();
                    }
                }
                else
                {
                    _sawmill.Error($"Failed to create audio stream for {_sourceEntity}");
                    _expectedChunkEndTime = null;
                    PlayNextChunk();
                }
            }
            catch (Exception ex)
            {
                _sawmill.Error($"Error playing voice audio for {_sourceEntity}: {ex.Message}");
                _expectedChunkEndTime = null;
                PlayNextChunk();
            }
        }
        else
        {
            if (pcmData != null && pcmData.Length == 0)
                _sawmill.Warning($"[{_gameTiming.CurTime.TotalSeconds:F3}] PlayNextChunk: Processed packet resulted in zero length for {_sourceEntity}. Skipping playback.");
            else if (pcmData == null)
                _sawmill.Error($"[{_gameTiming.CurTime.TotalSeconds:F3}] PlayNextChunk: Logic error - pcmData is null after processing for {_sourceEntity}.");

            _expectedChunkEndTime = null;
        }
    }

    /// <summary>
    /// Converts a byte array of PCM data to a short array for audio playback.
    /// </summary>
    private short[] ConvertToShortArray(byte[] byteArray)
    {
        int byteLength = byteArray.Length;
        if (byteLength % 2 != 0)
        {
            _sawmill.Warning($"ConvertToShortArray: Odd byte array length ({byteLength}). Truncating last byte.");
            byteLength--;
            if (byteLength < 0) return Array.Empty<short>();
        }

        int shortCount = byteLength / 2;
        short[] result = new short[shortCount];

        for (int i = 0; i < shortCount; i++)
        {
            int byteIndex = i * 2;
            result[i] = (short) ((byteArray[byteIndex + 1] << 8) | byteArray[byteIndex]);
        }

        return result;
    }


    /// <summary>
    /// Sets the volume for voice playback.
    /// </summary>
    public void SetVolume(float volume)
    {
        _sawmill.Info($"[VOICE VOLUME] SetVolume called for entity {_sourceEntity}: {_volume} -> {volume}");
        _volume = volume;
    }

    /// <summary>
    /// Disposes the voice stream manager and cleans up resources.
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed) return;

        _isDisposed = true;
        _isPlaying = false;
        _expectedChunkEndTime = null;

        _cfg.UnsubValueChanged(GoobCVars.VoiceChatBufferTargetMultiplier, OnBufferMultiplierChanged);
        _cfg.UnsubValueChanged(GoobCVars.VoiceChatMinBufferSize, OnMinBufferSizeChanged);
        _cfg.UnsubValueChanged(GoobCVars.VoiceChatMaxBufferSize, OnMaxBufferSizeChanged);
        _cfg.UnsubValueChanged(GoobCVars.VoiceChatDebugLogging, OnDebugLoggingChanged);
        _cfg.UnsubValueChanged(GoobCVars.VoiceChatVolume, OnVolumeChanged);

        lock (_queueLock)
        {
            _packetQueue.Clear();
        }

        _sawmill.Debug($"Disposed voice stream for entity {_sourceEntity}");
    }

    /// <summary>
    /// Called every frame to check if the next audio chunk should be played.
    /// </summary>
    public void Update()
    {
        if (_isDisposed || !_isPlaying)
            return;

        if (_expectedChunkEndTime == null)
        {
            bool canPlay = false;
            lock (_queueLock) { canPlay = _packetQueue.Count >= 1; }

            if (canPlay)
            {
                _sawmill.Debug($"[{_gameTiming.CurTime.TotalSeconds:F3}] Update: Expected end time is null for {_sourceEntity}, attempting PlayNextChunk.");
                PlayNextChunk();
            }
            else
            {
                _isPlaying = false;
                _sawmill.Debug($"[{_gameTiming.CurTime.TotalSeconds:F3}] Update: Setting _isPlaying = false due to insufficient packets in Update check.");
            }
            return;
        }

        if (_gameTiming.CurTime >= _expectedChunkEndTime)
        {
            _sawmill.Debug($"[{_gameTiming.CurTime.TotalSeconds:F3}] Update: Expected end time reached for {_sourceEntity}.");
            _expectedChunkEndTime = null;
            PlayNextChunk();
        }
    }
}

public enum BufferHealthState
{
    Critical,
    Low,
    Optimal,
    High,
    Overflow
}

public class PlaybackDecision
{
    public int PacketsToConsume { get; set; }
    public float TimeStretchRatio { get; set; }
    public string Mode { get; set; } = "";
    public bool UseInterpolation { get; set; }
}

public class NetworkConditionMonitor
{
    private readonly Queue<TimeSpan> _packetIntervals = new();
    private readonly int _maxSamples = 50;
    private TimeSpan _lastPacketTime;
    private TimeSpan _averageInterval = TimeSpan.FromMilliseconds(20);
    private TimeSpan _jitter = TimeSpan.Zero;
    private float _bufferMultiplier;
    private int _minBufferSize;
    private int _maxBufferSize;

    public NetworkConditionMonitor(float bufferMultiplier, int minBufferSize, int maxBufferSize)
    {
        _bufferMultiplier = bufferMultiplier;
        _minBufferSize = minBufferSize;
        _maxBufferSize = maxBufferSize;
    }

    public void UpdateSettings(float bufferMultiplier, int minBufferSize, int maxBufferSize)
    {
        _bufferMultiplier = bufferMultiplier;
        _minBufferSize = minBufferSize;
        _maxBufferSize = maxBufferSize;
    }

    public void RecordPacketArrival(TimeSpan currentTime)
    {
        if (_lastPacketTime != TimeSpan.Zero)
        {
            var interval = currentTime - _lastPacketTime;
            _packetIntervals.Enqueue(interval);

            if (_packetIntervals.Count > _maxSamples)
                _packetIntervals.Dequeue();

            UpdateStatistics();
        }
        _lastPacketTime = currentTime;
    }

    private void UpdateStatistics()
    {
        if (_packetIntervals.Count == 0) return;

        var intervals = _packetIntervals.ToArray();
        _averageInterval = TimeSpan.FromMilliseconds(intervals.Average(i => i.TotalMilliseconds));

        var variance = intervals.Select(i => Math.Pow(i.TotalMilliseconds - _averageInterval.TotalMilliseconds, 2)).Average();
        _jitter = TimeSpan.FromMilliseconds(Math.Sqrt(variance));
    }

    public int CalculateTargetBufferSize()
    {
        if (_packetIntervals.Count < 5)
            return Math.Max(8, _minBufferSize / 3);

        var avgIntervalMs = _averageInterval.TotalMilliseconds;
        var jitterMs = _jitter.TotalMilliseconds;

        var baseBufferTimeMs = 100.0;
        var packetsIn100ms = Math.Max(3, baseBufferTimeMs / Math.Max(20.0, avgIntervalMs));

        var jitterPackets = Math.Max(1, jitterMs / 20.0);

        var targetSize = (int) ((packetsIn100ms + jitterPackets) * Math.Min(_bufferMultiplier, 2.0));

        var effectiveMin = Math.Min(_minBufferSize, 15);
        var effectiveMax = Math.Min(_maxBufferSize, 30);

        return Math.Clamp(targetSize, effectiveMin, effectiveMax);
    }

    public TimeSpan AverageInterval => _averageInterval;
    public TimeSpan Jitter => _jitter;
}

public class AdaptiveBufferThresholds
{
    private int _targetBufferSize = 20;
    private readonly NetworkConditionMonitor _networkMonitor;

    public AdaptiveBufferThresholds(NetworkConditionMonitor networkMonitor)
    {
        _networkMonitor = networkMonitor;
    }

    public int EmergencyThreshold => (int) (_targetBufferSize * 0.3);
    public int StretchEnterThreshold => (int) (_targetBufferSize * 0.6);
    public int StretchExitThreshold => (int) (_targetBufferSize * 0.8);
    public int CompressEnterThreshold => (int) (_targetBufferSize * 1.4);
    public int CompressExitThreshold => (int) (_targetBufferSize * 1.2);
    public int AggressiveThreshold => (int) (_targetBufferSize * 2.0);

    public void UpdateTarget()
    {
        _targetBufferSize = _networkMonitor.CalculateTargetBufferSize();
    }

    public BufferHealthState GetBufferState(int currentSize)
    {
        if (currentSize < EmergencyThreshold) return BufferHealthState.Critical;
        if (currentSize < StretchEnterThreshold) return BufferHealthState.Low;
        if (currentSize < CompressEnterThreshold) return BufferHealthState.Optimal;
        if (currentSize < AggressiveThreshold) return BufferHealthState.High;
        return BufferHealthState.Overflow;
    }
}

public class AdaptivePlaybackEngine
{
    private BufferHealthState _currentState = BufferHealthState.Optimal;
    private readonly AdaptiveBufferThresholds _thresholds;
    private readonly ISawmill _sawmill;
    private bool _debugLogging;
    private int _modeChangeCount = 0;
    private TimeSpan _lastModeChange = TimeSpan.Zero;

    public AdaptivePlaybackEngine(AdaptiveBufferThresholds thresholds, ISawmill sawmill, bool debugLogging = false)
    {
        _thresholds = thresholds;
        _sawmill = sawmill;
        _debugLogging = debugLogging;
    }

    public void UpdateDebugLogging(bool debugLogging)
    {
        _debugLogging = debugLogging;
    }

    public PlaybackDecision DecidePlaybackStrategy(int queueSize)
    {
        var newState = _thresholds.GetBufferState(queueSize);

        if (ShouldChangeState(_currentState, newState, queueSize))
        {
            _modeChangeCount++;
            var currentTime = IoCManager.Resolve<IGameTiming>().CurTime;
            var timeSinceLastChange = currentTime - _lastModeChange;
            _lastModeChange = currentTime;

            if (_debugLogging)
            {
                _sawmill.Info($"[VOICE BUFFER] State change #{_modeChangeCount}: {_currentState} -> {newState} (Queue: {queueSize}, Time since last: {timeSinceLastChange.TotalSeconds:F1}s)");
                _sawmill.Info($"[VOICE BUFFER] Thresholds - Emergency: {_thresholds.EmergencyThreshold}, Stretch: {_thresholds.StretchEnterThreshold}-{_thresholds.StretchExitThreshold}, Compress: {_thresholds.CompressEnterThreshold}-{_thresholds.CompressExitThreshold}");
            }
            else
            {
                _sawmill.Debug($"Buffer state changing from {_currentState} to {newState} (Queue: {queueSize})");
            }

            _currentState = newState;
        }

        return CreatePlaybackDecision(_currentState, queueSize);
    }

    private bool ShouldChangeState(BufferHealthState current, BufferHealthState proposed, int queueSize)
    {
        switch (current)
        {
            case BufferHealthState.Low when proposed == BufferHealthState.Optimal:
                return queueSize >= _thresholds.StretchExitThreshold;
            case BufferHealthState.Optimal when proposed == BufferHealthState.Low:
                return queueSize <= _thresholds.StretchEnterThreshold;
            case BufferHealthState.Optimal when proposed == BufferHealthState.High:
                return queueSize >= _thresholds.CompressEnterThreshold;
            case BufferHealthState.High when proposed == BufferHealthState.Optimal:
                return queueSize <= _thresholds.CompressExitThreshold;
            default:
                return proposed != current;
        }
    }

    private PlaybackDecision CreatePlaybackDecision(BufferHealthState state, int queueSize)
    {
        return state switch
        {
            BufferHealthState.Critical => new PlaybackDecision
            {
                PacketsToConsume = Math.Min(1, queueSize),
                TimeStretchRatio = 1.1f,
                Mode = "Emergency",
                UseInterpolation = true
            },
            BufferHealthState.Low => new PlaybackDecision
            {
                PacketsToConsume = queueSize >= 3 ? 1 : Math.Min(1, queueSize),
                TimeStretchRatio = 1.15f,
                Mode = "Stretch"
            },
            BufferHealthState.Optimal => new PlaybackDecision
            {
                PacketsToConsume = Math.Min(2, queueSize),
                TimeStretchRatio = 1.0f,
                Mode = "Normal"
            },
            BufferHealthState.High => new PlaybackDecision
            {
                PacketsToConsume = Math.Min(2, queueSize),
                TimeStretchRatio = 0.9f,
                Mode = "Compress"
            },
            BufferHealthState.Overflow => new PlaybackDecision
            {
                PacketsToConsume = Math.Min(3, queueSize),
                TimeStretchRatio = 0.8f,
                Mode = "Aggressive"
            },
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
