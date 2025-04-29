using Robust.Client.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Voice;
public sealed class VoiceStreamManager : IDisposable
{
    private enum PlaybackMode { Stretch, Normal, Compress }

    private readonly Queue<byte[]> _packetQueue = new();
    private readonly object _queueLock = new();
    private bool _isPlaying;
    private bool _isDisposed;
    private TimeSpan? _expectedChunkEndTime;
    private PlaybackMode _currentPlaybackMode = PlaybackMode.Normal;

    private readonly int _sampleRate;
    private readonly IAudioManager _audioManager;
    private readonly AudioSystem _audioSystem;
    private readonly EntityUid _sourceEntity;
    private readonly ISawmill _sawmill;
    private readonly IGameTiming _gameTiming;

    private const int BytesPerSample = 2;
    private const int Channels = 1;
    private const int PacketsPerChunk = 2;
    private readonly int _maxQueuedPackets = 50;
    private readonly int _mergeThresholdPackets = 30;
    private readonly int _stretchEnterThreshold = 6;
    private readonly int _stretchExitThreshold = 14;
    private const float CompressRatio = 0.75f;
    private const float StretchRatio = 1.25f;
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
    }

    /// <summary>
    /// Adds a packet of PCM audio data to the playback queue.
    /// </summary>
    public void AddPacket(byte[] pcmData)
    {
        if (_isDisposed) return;

        lock (_queueLock)
        {
            if (_packetQueue.Count >= _maxQueuedPackets)
            {
                _sawmill.Warning($"[{_gameTiming.CurTime.TotalSeconds:F3}] Voice buffer full for {_sourceEntity} (Queue: {_packetQueue.Count}/{_maxQueuedPackets}). Dropping packet ({pcmData.Length} bytes).");
                return;
            }

            var dataCopy = new byte[pcmData.Length];
            Array.Copy(pcmData, dataCopy, pcmData.Length);
            _packetQueue.Enqueue(dataCopy);
            _sawmill.Debug($"[{_gameTiming.CurTime.TotalSeconds:F3}] AddPacket: Packet received for {_sourceEntity} ({pcmData.Length} bytes). Queue size now: {_packetQueue.Count}/{_maxQueuedPackets}");

            if (!_isPlaying && _packetQueue.Count >= PacketsPerChunk)
            {
                _sawmill.Debug($"[{_gameTiming.CurTime.TotalSeconds:F3}] Sufficient packets ({_packetQueue.Count}/{PacketsPerChunk}) for chunk reached for {_sourceEntity}. Starting playback.");
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
    /// Plays the next chunk of audio data. Always processes 4 packets.
    /// Compresses, stretches, or plays normally based on buffer size.
    /// </summary>
    private void PlayNextChunk()
    {
        if (_isDisposed) return;

        byte[]? pcmData = null;

        lock (_queueLock)
        {
            int queueCount = _packetQueue.Count;

            if (queueCount < PacketsPerChunk)
            {
                _sawmill.Debug($"[{_gameTiming.CurTime.TotalSeconds:F3}] PlayNextChunk: Not enough packets ({queueCount}/{PacketsPerChunk}) for {_sourceEntity}. Stopping playback flag.");
                _isPlaying = false;
                _expectedChunkEndTime = null;
                _sawmill.Debug($"[{_gameTiming.CurTime.TotalSeconds:F3}] PlayNextChunk: Setting _isPlaying = false due to insufficient packets."); // Added log
                return;
            }

            _sawmill.Debug($"[{_gameTiming.CurTime.TotalSeconds:F3}] PlayNextChunk: Queue count before mode decision: {queueCount}. Current Mode: {_currentPlaybackMode}"); // Added log

            string mode;
            if (_currentPlaybackMode == PlaybackMode.Stretch)
            {
                if (queueCount >= _stretchExitThreshold)
                {
                    _currentPlaybackMode = PlaybackMode.Normal;
                    mode = "Normal (Exit Stretch)";
                }
                else
                {
                    mode = "Stretch (Stay)";
                }
            }
            else
            {
                if (queueCount < _stretchEnterThreshold)
                {
                    _currentPlaybackMode = PlaybackMode.Stretch;
                    mode = "Stretch (Enter)";
                }
                else if (queueCount >= _mergeThresholdPackets)
                {
                    _currentPlaybackMode = PlaybackMode.Compress;
                    mode = "Compress";
                }
                else
                {
                    _currentPlaybackMode = PlaybackMode.Normal;
                    mode = "Normal";
                }
            }

            float processingRatio;
            switch (_currentPlaybackMode)
            {
                case PlaybackMode.Stretch:
                    processingRatio = StretchRatio;
                    break;
                case PlaybackMode.Compress:
                    processingRatio = CompressRatio;
                    break;
                default:
                    processingRatio = 1.0f;
                    break;
            }

            var packetsToProcess = new List<byte[]>(PacketsPerChunk);
            for (int i = 0; i < PacketsPerChunk; i++)
                packetsToProcess.Add(_packetQueue.Dequeue());

            pcmData = ProcessPackets(packetsToProcess, processingRatio);

            _sawmill.Debug($"[{_gameTiming.CurTime.TotalSeconds:F3}] Decided Mode: {mode}. Processing ({_currentPlaybackMode} Ratio {processingRatio:F2}) {PacketsPerChunk} packets for {_sourceEntity}. Queue size now: {_packetQueue.Count}/{_maxQueuedPackets}");
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
            lock (_queueLock) { canPlay = _packetQueue.Count >= PacketsPerChunk; }

            if (canPlay)
            {
                _sawmill.Debug($"[{_gameTiming.CurTime.TotalSeconds:F3}] Update: Expected end time is null for {_sourceEntity}, attempting PlayNextChunk.");
                PlayNextChunk();
            }
            else
            {
                _isPlaying = false;
                _sawmill.Debug($"[{_gameTiming.CurTime.TotalSeconds:F3}] Update: Setting _isPlaying = false due to insufficient packets in Update check."); // Added log
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
