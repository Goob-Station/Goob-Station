using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Robust.Client.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Voice;

public sealed class VoiceStreamManager : IDisposable
{
    private enum PlaybackMode { Stretch, Normal, Compress }

    [Dependency] private readonly IAudioManager _audioManager = default!;
    [Dependency] private readonly AudioSystem _audioSystem = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;

    private readonly Queue<byte[]> _packetQueue = new();
    private readonly object _queueLock = new();
    private bool _isDisposed;
    private PlaybackMode _currentPlaybackMode = PlaybackMode.Normal;
    private CancellationTokenSource? _playbackTimerCts;
    private bool _isPlaying = false;
    private bool _isTimerCallbackRunning = false;

    private const int SampleRate = 48000;
    private const int BytesPerSample = 2;
    private const int Channels = 1;
    private const int PacketsPerChunk = 5;
    private readonly int _maxQueuedPackets = 50;
    private readonly int _mergeThresholdPackets = 30;
    private readonly int _stretchEnterThreshold = 6;
    private readonly int _stretchExitThreshold = 10;
    private const float CompressRatio = 0.75f;
    private const float StretchRatio = 1.25f;
    private float _volume = 0.5f;
    private readonly TimeSpan _timerSafetyMargin = TimeSpan.FromMilliseconds(0);

    private readonly EntityUid _sourceEntity;
    private readonly ISawmill _sawmill;

    public VoiceStreamManager(EntityUid sourceEntity)
    {
        IoCManager.InjectDependencies(this);

        _sourceEntity = sourceEntity;
        _sawmill = Logger.GetSawmill("voice.stream");
        _sawmill.Debug($"Initialized VoiceStreamManager for entity {_sourceEntity}.");
    }

    public void AddPacket(byte[] pcmData)
    {
        if (_isDisposed) return;

        lock (_queueLock)
        {
            if (_packetQueue.Count >= _maxQueuedPackets)
            {
                _sawmill.Warning($"[{_gameTiming.CurTime.TotalSeconds:F3}] Voice packet queue full for {_sourceEntity} (Queue: {_packetQueue.Count}/{_maxQueuedPackets}). Dropping packet ({pcmData.Length} bytes).");
                return;
            }

            var dataCopy = new byte[pcmData.Length];
            Array.Copy(pcmData, dataCopy, pcmData.Length);
            _packetQueue.Enqueue(dataCopy);
            _sawmill.Debug($"[{_gameTiming.CurTime.TotalSeconds:F3}] AddPacket: Packet received. Queue size now: {_packetQueue.Count}");

            if (!_isPlaying && _packetQueue.Count >= PacketsPerChunk)
            {
                _sawmill.Info($"[{_gameTiming.CurTime.TotalSeconds:F3}] Sufficient packets ({_packetQueue.Count}/{PacketsPerChunk}) received. Starting playback cycle for {_sourceEntity}.");
                _isPlaying = true;
                Task.Run(PlayNextChunk);
            }
        }
    }

    private void PlayNextChunk()
    {
        if (_isDisposed || Interlocked.Exchange(ref _isTimerCallbackRunning, true))
        {
            if (!_isDisposed) _sawmill.Warning($"[{_gameTiming.CurTime.TotalSeconds:F3}] PlayNextChunk re-entrancy detected for {_sourceEntity}. Skipping.");
            return;
        }

        try
        {
            byte[]? pcmData = null;
            TimeSpan actualDuration = TimeSpan.Zero;
            float processingRatio = 1.0f;
            string mode = "Normal";
            bool shouldContinuePlayback = false;

            lock (_queueLock)
            {
                int queueCount = _packetQueue.Count;

                if (queueCount < PacketsPerChunk)
                {
                    _sawmill.Debug($"[{_gameTiming.CurTime.TotalSeconds:F3}] PlayNextChunk: Not enough packets ({queueCount}/{PacketsPerChunk}) for {_sourceEntity}. Stopping playback cycle.");
                    _isPlaying = false;
                    return;
                }

                if (_currentPlaybackMode == PlaybackMode.Stretch)
                {
                    if (queueCount >= _stretchExitThreshold) _currentPlaybackMode = PlaybackMode.Normal;
                }
                else
                {
                    if (queueCount < _stretchEnterThreshold) _currentPlaybackMode = PlaybackMode.Stretch;
                    else if (queueCount >= _mergeThresholdPackets) _currentPlaybackMode = PlaybackMode.Compress;
                    else _currentPlaybackMode = PlaybackMode.Normal;
                }

                processingRatio = _currentPlaybackMode switch
                {
                    PlaybackMode.Stretch => StretchRatio,
                    PlaybackMode.Compress => CompressRatio,
                    _ => 1.0f,
                };
                mode = _currentPlaybackMode.ToString();

                var packetsToProcess = new List<byte[]>(PacketsPerChunk);
                for (int i = 0; i < PacketsPerChunk; i++)
                    packetsToProcess.Add(_packetQueue.Dequeue());

                _sawmill.Debug($"[{_gameTiming.CurTime.TotalSeconds:F3}] PlayNextChunk: Dequeued {PacketsPerChunk} packets. Mode: {mode} (Ratio: {processingRatio:F2}). Queue size now: {_packetQueue.Count}.");

                pcmData = ProcessPackets(packetsToProcess, processingRatio);
                shouldContinuePlayback = true;
            }

            if (shouldContinuePlayback && pcmData != null && pcmData.Length > 0)
            {
                try
                {
                    actualDuration = TimeSpan.FromSeconds((double) pcmData.Length / (SampleRate * Channels * BytesPerSample));
                    if (actualDuration <= TimeSpan.Zero)
                    {
                        _sawmill.Warning($"[{_gameTiming.CurTime.TotalSeconds:F3}] Calculated zero or negative duration ({actualDuration.TotalMilliseconds}ms) for chunk. Skipping playback.");
                        _isPlaying = false;
                        return;
                    }
                    _sawmill.Debug($"[{_gameTiming.CurTime.TotalSeconds:F3}] Calculated Actual Duration: {actualDuration.TotalMilliseconds:F1}ms for {pcmData.Length} bytes.");

                    short[] shortArray = ConvertToShortArray(pcmData);
                    var audioStream = _audioManager.LoadAudioRaw(shortArray, Channels, SampleRate);

                    if (audioStream != null)
                    {
                        var audioParams = AudioParams.Default.WithVolume(_volume).WithMaxDistance(10f);

                        if (!_entityManager.EntityExists(_sourceEntity))
                        {
                            _sawmill.Warning($"[{_gameTiming.CurTime.TotalSeconds:F3}] Entity {_sourceEntity} no longer exists. Stopping playback cycle.");
                            _isPlaying = false;
                            Dispose();
                            return;
                        }

                        var playResult = _audioSystem.PlayEntity(audioStream, _sourceEntity, null, audioParams);

                        if (playResult != null)
                        {
                            _sawmill.Debug($"[{_gameTiming.CurTime.TotalSeconds:F3}] Playing chunk for {_sourceEntity}. Actual Duration: {actualDuration.TotalMilliseconds:F1}ms.");
                            ScheduleNextChunkTimer(actualDuration);
                        }
                        else
                        {
                            _sawmill.Warning($"[{_gameTiming.CurTime.TotalSeconds:F3}] Failed to play audio via AudioSystem for {_sourceEntity}. Stopping playback cycle.");
                            _isPlaying = false;
                        }
                    }
                    else
                    {
                        _sawmill.Error($"[{_gameTiming.CurTime.TotalSeconds:F3}] Failed to create audio stream from raw data for {_sourceEntity}. Stopping playback cycle.");
                        _isPlaying = false;
                    }
                }
                catch (Exception ex)
                {
                    _sawmill.Error($"[{_gameTiming.CurTime.TotalSeconds:F3}] Error playing voice audio chunk for {_sourceEntity}: {ex.Message}");
                    _isPlaying = false;
                }
            }
            else
            {
                _sawmill.Warning($"[{_gameTiming.CurTime.TotalSeconds:F3}] PlayNextChunk: Processed packet resulted in zero length or null for {_sourceEntity}. Stopping playback cycle.");
                _isPlaying = false;
            }
        }
        finally
        {
            Interlocked.Exchange(ref _isTimerCallbackRunning, false);
        }
    }

    private void ScheduleNextChunkTimer(TimeSpan delay)
    {
        if (_isDisposed) return;

        _playbackTimerCts?.Cancel();
        _playbackTimerCts?.Dispose();
        _playbackTimerCts = new CancellationTokenSource();
        var token = _playbackTimerCts.Token;

        var timerDelay = delay - _timerSafetyMargin;
        if (timerDelay < TimeSpan.Zero)
            timerDelay = TimeSpan.Zero;

        _sawmill.Debug($"[{_gameTiming.CurTime.TotalSeconds:F3}] Scheduling next chunk timer with delay: {timerDelay.TotalMilliseconds:F1}ms (Original: {delay.TotalMilliseconds:F1}ms)");

        Robust.Shared.Timing.Timer.Delay(timerDelay, token).ContinueWith(task =>
        {
            if (token.IsCancellationRequested || _isDisposed)
            {
                _sawmill.Debug($"[{_gameTiming.CurTime.TotalSeconds:F3}] Playback timer callback cancelled or object disposed.");
                _isPlaying = false;
                return;
            }

            if (task.IsCompletedSuccessfully)
            {
                PlayNextChunk();
            }
            else if (task.IsCanceled)
            {
                _sawmill.Debug($"[{_gameTiming.CurTime.TotalSeconds:F3}] Playback timer task was cancelled.");
                _isPlaying = false;
            }
            else if (task.IsFaulted)
            {
                _sawmill.Error($"[{_gameTiming.CurTime.TotalSeconds:F3}] Playback timer task faulted: {task.Exception}");
                _isPlaying = false;
            }

        }, TaskScheduler.Default);
    }

    private byte[] ProcessPackets(List<byte[]> packetsToProcess, float ratio)
    {
        int totalBytes = packetsToProcess.Sum(p => p.Length);

        if (Math.Abs(ratio - 1.0f) < 0.001f)
        {
            byte[] concatResult = new byte[totalBytes];
            int offset = 0;
            foreach (var packet in packetsToProcess)
            {
                for (int i = 0; i < packet.Length; i++)
                {
                    if (offset + i < concatResult.Length)
                        concatResult[offset + i] = packet[i];
                    else
                        _sawmill.Warning("ProcessPackets concatenation bounds error.");
                }
                offset += packet.Length;
            }
            return concatResult;
        }

        int totalSamples = totalBytes / BytesPerSample;
        int targetSamples = (int) (totalSamples / ratio);
        byte[] result = new byte[targetSamples * BytesPerSample];
        int resultIndex = 0;

        for (int i = 0; i < targetSamples; i++)
        {
            float sourceSampleFloat = i * ratio;
            int sourceSampleIndex1 = (int) sourceSampleFloat;
            int sourceSampleIndex2 = sourceSampleIndex1 + 1;
            float fraction = sourceSampleFloat - sourceSampleIndex1;

            if (!TryGetSample(packetsToProcess, sourceSampleIndex1, out short sample1))
            {
                sample1 = 0;
                if (TryGetSample(packetsToProcess, totalSamples - 1, out short lastSample)) sample1 = lastSample;
                _sawmill.Warning($"ProcessPackets: source index {sourceSampleIndex1} out of bounds (total: {totalSamples}). Using last sample.");
            }

            short finalSample;
            if (fraction > 0.001f && sourceSampleIndex2 < totalSamples && TryGetSample(packetsToProcess, sourceSampleIndex2, out short sample2))
                finalSample = (short) (sample1 + (sample2 - sample1) * fraction);
            else
                finalSample = sample1;

            if (resultIndex < result.Length - 1)
            {
                result[resultIndex++] = (byte) (finalSample & 0xFF);
                result[resultIndex++] = (byte) ((finalSample >> 8) & 0xFF);
            }
            else
            {
                _sawmill.Warning($"ProcessPackets calculation error: result index {resultIndex} out of bounds for result length {result.Length}. Ratio: {ratio}");
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

    private bool TryGetSample(List<byte[]> packets, int globalSampleIndex, out short sample)
    {
        sample = 0;
        if (globalSampleIndex < 0) return false;

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

    private short[] ConvertToShortArray(byte[] byteArray)
    {
        int byteLength = byteArray.Length;
        if (byteLength % 2 != 0)
        {
            _sawmill.Warning($"ConvertToShortArray: Odd byte array length ({byteLength}). Truncating last byte.");
            byteLength--;
            if (byteLength <= 0) return Array.Empty<short>();
        }

        int shortCount = byteLength / 2;
        short[] result = new short[shortCount];

        for (int i = 0; i < shortCount; i++)
        {
            int byteIndex = i * 2;
            byte b1 = byteArray[byteIndex];
            byte b2 = byteArray[byteIndex + 1];
            result[i] = (short) ((b2 << 8) | b1);
        }

        return result;
    }

    public void SetVolume(float volume)
    {
        _volume = Math.Clamp(volume, 0f, 1f);
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        _sawmill.Debug($"Disposing voice stream for entity {_sourceEntity}");

        _playbackTimerCts?.Cancel();
        _playbackTimerCts?.Dispose();
        _playbackTimerCts = null;

        _isPlaying = false;

        lock (_queueLock)
        {
            _packetQueue.Clear();
        }
    }
}
