using System.Threading.Tasks;
using Robust.Client.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Voice;

/// <summary>
/// Manages the buffering and playback of voice audio for a single entity.
/// </summary>
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

    private readonly int _initialBufferPackets = 3;
    private readonly int _maxQueuedPackets = 50;
    private float _volume = 0.5f;

    public VoiceStreamManager(IAudioManager audioManager, AudioSystem audioSystem,
                             EntityUid sourceEntity, int sampleRate)
    {
        _audioManager = audioManager;
        _audioSystem = audioSystem;
        _sourceEntity = sourceEntity;
        _sampleRate = sampleRate;
        _sawmill = Logger.GetSawmill("voiceclient");
        _gameTiming = IoCManager.Resolve<IGameTiming>(); // Only for testing purposes
    }

    /// <summary>
    /// Adds a packet of PCM audio data to the playback queue.
    /// </summary>
    /// <param name="pcmData">The PCM audio data to add.</param>
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
            for (int i = 0; i < pcmData.Length; i++)
            {
                dataCopy[i] = pcmData[i];
            }

            _packetQueue.Enqueue(dataCopy);
            _sawmill.Debug($"[{_gameTiming.CurTime.TotalSeconds:F3}] Packet received for {_sourceEntity} ({pcmData.Length} bytes). Queue size: {_packetQueue.Count}/{_maxQueuedPackets}");

            if (!_isPlaying && _packetQueue.Count >= _initialBufferPackets)
            {
                _isPlaying = true;
                PlayNextChunk();
            }
        }
    }

    /// <summary>
    /// Plays the next chunk of audio data from the queue.
    /// </summary>
    private void PlayNextChunk()
    {
        if (_isDisposed) return;

        byte[] pcmData;
        lock (_queueLock)
        {
            if (_packetQueue.Count == 0)
            {
                _isPlaying = false;
                return;
            }

            pcmData = _packetQueue.Dequeue();
            _sawmill.Debug($"[{_gameTiming.CurTime.TotalSeconds:F3}] Dequeuing packet for {_sourceEntity} ({pcmData.Length} bytes). Queue size: {_packetQueue.Count}/{_maxQueuedPackets}");
        }

        try
        {
            short[] shortArray = ConvertToShortArray(pcmData);

            var audioStream = _audioManager.LoadAudioRaw(shortArray, 1, _sampleRate);
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
                    _expectedChunkEndTime = _gameTiming.CurTime + TimeSpan.FromMilliseconds(20);
                }
                else
                {
                    _sawmill.Warning($"Failed to play audio for {_sourceEntity}");
                    PlayNextChunk();
                }
            }
            else
            {
                _sawmill.Error($"Failed to create audio stream for {_sourceEntity}");
                PlayNextChunk();
            }
        }
        catch (Exception ex)
        {
            _sawmill.Error($"Error playing voice audio: {ex.Message}");
            PlayNextChunk();
        }
    }

    /// <summary>
    /// Converts a byte array of PCM data to a short array for audio playback.
    /// </summary>
    private short[] ConvertToShortArray(byte[] byteArray)
    {
        int shortCount = byteArray.Length / 2;
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

        lock (_queueLock)
        {
            _packetQueue.Clear();
        }

        _sawmill.Debug($"Disposed voice stream for entity {_sourceEntity}");
    }

    public void Update()
    {
        if (_isDisposed || !_isPlaying || _expectedChunkEndTime == null)
            return;

        if (_gameTiming.CurTime >= _expectedChunkEndTime)
        {
            _expectedChunkEndTime = null;
            PlayNextChunk();
        }
    }
}
