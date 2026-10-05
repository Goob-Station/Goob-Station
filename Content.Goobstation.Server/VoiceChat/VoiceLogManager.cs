using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Content.Goobstation.Shared.VoiceChat;
using Robust.Shared.ContentPack;

namespace Content.Goobstation.Server.VoiceChat;

public sealed class VoiceLogManager
{
    [Dependency] private readonly IResourceManager _resources = default!;
    [Dependency] private readonly ILogManager _logManager = default!;

    private const string DirectoryName = "voice_logs";
    private const string FileExtension = ".vlog";
    private const string RoundFile = "round.meta";
    private const byte Version = 1;
    private const byte FrameRecord = 1;
    private const byte MetaRecord = 2;
    private const int FrameRecordBytes = 1 + 8 + 2 + 1 + 1 + VoiceCodec.FrameBytes;
    private const long SegmentGapMs = 300;
    private const int MaxSequenceGap = 25;
    private const int MaxSegmentFrames = 3000;
    private const int MaxAudioFrames = 3000;

    private static readonly byte[] Magic = "GVL1"u8.ToArray();

    private readonly BlockingCollection<Action> _writes = new();
    private readonly Dictionary<(int Round, Guid User), Writer> _writers = new();
    private readonly ConcurrentDictionary<(int Round, Guid User), TrackIndex> _indices = new();
    private readonly short[] _levelPcm = new short[VoiceCodec.FrameSamples];

    private ISawmill _sawmill = default!;
    private string? _root;
    private Thread? _thread;

    public bool Available => _root != null;

    public void Initialize()
    {
        _sawmill = _logManager.GetSawmill("voice.logs");

        if (_resources.UserData.RootDir is not { } rootDir)
        {
            _sawmill.Warning("Voice logs disabled: the server has no on-disk data directory.");
            return;
        }

        _root = Path.Combine(rootDir, DirectoryName);
        Directory.CreateDirectory(_root);

        _thread = new Thread(RunWriter)
        {
            Name = "Voice log writer",
            IsBackground = true,
        };
        _thread.Start();
    }

    public void Shutdown()
    {
        _writes.CompleteAdding();
        _thread?.Join(TimeSpan.FromSeconds(5));
    }

    public void Record(int round, Guid user, string username, long timeMs, ushort sequence, VoiceLogFlags flags, byte[] payload, string name, string channel)
    {
        if (_root == null || _writes.IsAddingCompleted || payload.Length != VoiceCodec.FrameBytes)
            return;

        _writes.Add(() => WriteFrame(round, user, username, timeMs, sequence, flags, payload, name, channel));
    }

    public void StartRound(int round, long startMs, int keepRounds)
    {
        if (_root == null || _writes.IsAddingCompleted)
            return;

        _writes.Add(() => BeginRound(round, startMs, keepRounds));
    }

    public Task<List<VoiceLogRound>> ListRounds(int currentRound)
    {
        return Task.Run(() =>
        {
            var rounds = new List<VoiceLogRound>();
            if (_root == null)
                return rounds;

            foreach (var directory in Directory.EnumerateDirectories(_root))
            {
                if (!int.TryParse(Path.GetFileName(directory), out var round))
                    continue;

                var start = ReadRoundStart(directory);
                var end = start;
                foreach (var file in Directory.EnumerateFiles(directory, "*" + FileExtension))
                {
                    end = Math.Max(end, new DateTimeOffset(File.GetLastWriteTimeUtc(file)).ToUnixTimeMilliseconds());
                }

                rounds.Add(new VoiceLogRound(round, start, end, round == currentRound));
            }

            rounds.Sort((a, b) => b.RoundId.CompareTo(a.RoundId));
            return rounds;
        });
    }

    public Task<List<VoiceLogSpeaker>> ListSpeakers(int round)
    {
        return Task.Run(() =>
        {
            var speakers = new List<VoiceLogSpeaker>();
            if (_root == null)
                return speakers;

            var directory = Path.Combine(_root, round.ToString());
            if (!Directory.Exists(directory))
                return speakers;

            foreach (var file in Directory.EnumerateFiles(directory, "*" + FileExtension))
            {
                if (!Guid.TryParse(Path.GetFileNameWithoutExtension(file), out var user))
                    continue;

                var index = GetIndex(round, user);
                lock (index)
                {
                    Parse(index, file);
                    speakers.Add(new VoiceLogSpeaker(user, index.Username, index.TotalFrames * VoiceCodec.FrameSamples / (float) VoiceCodec.SampleRate));
                }
            }

            speakers.Sort((a, b) => string.Compare(a.Username, b.Username, StringComparison.OrdinalIgnoreCase));
            return speakers;
        });
    }

    public Task<(string Username, List<VoiceLogSegment> Segments)?> ReadTrack(int round, Guid user)
    {
        return Task.Run<(string, List<VoiceLogSegment>)?>(() =>
        {
            if (TrackPath(round, user) is not { } path || !File.Exists(path))
                return null;

            var index = GetIndex(round, user);
            lock (index)
            {
                Parse(index, path);
                var segments = new List<VoiceLogSegment>(index.Segments.Count);
                foreach (var segment in index.Segments)
                {
                    segments.Add(new VoiceLogSegment(segment.StartMs, segment.Flags, segment.Name, segment.Channel, segment.Levels.ToArray()));
                }

                return (index.Username, segments);
            }
        });
    }

    public Task<List<byte[]>> ReadAudio(int round, Guid user, int firstSegment, int lastSegment)
    {
        return Task.Run(() =>
        {
            var payloads = new List<byte[]>();
            if (TrackPath(round, user) is not { } path || !File.Exists(path))
                return payloads;

            var index = GetIndex(round, user);
            lock (index)
            {
                Parse(index, path);
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);

                var frames = 0;
                for (var i = Math.Max(0, firstSegment); i <= lastSegment && i < index.Segments.Count; i++)
                {
                    var segment = index.Segments[i];
                    if (payloads.Count > 0 && frames + segment.Offsets.Count > MaxAudioFrames)
                        break;

                    var payload = new byte[segment.Offsets.Count * VoiceCodec.FrameBytes];
                    for (var slot = 0; slot < segment.Offsets.Count; slot++)
                    {
                        var offset = segment.Offsets[slot];
                        if (offset < 0)
                            continue;

                        stream.Position = offset;
                        stream.ReadExactly(payload, slot * VoiceCodec.FrameBytes, VoiceCodec.FrameBytes);
                    }

                    payloads.Add(payload);
                    frames += segment.Offsets.Count;
                }
            }

            return payloads;
        });
    }

    private void RunWriter()
    {
        foreach (var action in _writes.GetConsumingEnumerable())
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                _sawmill.Error($"Voice log write failed: {e}");
            }

            if (_writes.Count == 0)
                FlushWriters();
        }

        foreach (var writer in _writers.Values)
        {
            writer.Dispose();
        }

        _writers.Clear();
    }

    private void WriteFrame(int round, Guid user, string username, long timeMs, ushort sequence, VoiceLogFlags flags, byte[] payload, string name, string channel)
    {
        var writer = GetWriter(round, user, username);
        if (writer.Name != name || writer.Channel != channel)
        {
            writer.Output.Write(MetaRecord);
            writer.Output.Write(name);
            writer.Output.Write(channel);
            writer.Name = name;
            writer.Channel = channel;
        }

        writer.Output.Write(FrameRecord);
        writer.Output.Write(timeMs);
        writer.Output.Write(sequence);
        writer.Output.Write((byte) flags);
        writer.Output.Write(MeasureLevel(payload));
        writer.Output.Write(payload);
    }

    private void BeginRound(int round, long startMs, int keepRounds)
    {
        var directory = Path.Combine(_root!, round.ToString());
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, RoundFile), startMs.ToString());

        foreach (var key in _writers.Keys.Where(key => key.Round != round).ToList())
        {
            _writers[key].Dispose();
            _writers.Remove(key);
        }

        var rounds = Directory.EnumerateDirectories(_root!)
            .Select(path => (Path: path, Id: int.TryParse(Path.GetFileName(path), out var id) ? id : (int?) null))
            .Where(entry => entry.Id != null)
            .OrderByDescending(entry => entry.Id)
            .ToList();

        foreach (var (path, id) in rounds.Skip(Math.Max(1, keepRounds)))
        {
            if (id == round)
                continue;

            foreach (var key in _writers.Keys.Where(key => key.Round == id).ToList())
            {
                _writers[key].Dispose();
                _writers.Remove(key);
            }

            foreach (var key in _indices.Keys.Where(key => key.Round == id).ToList())
            {
                _indices.TryRemove(key, out _);
            }

            try
            {
                Directory.Delete(path, true);
            }
            catch (IOException e)
            {
                _sawmill.Warning($"Could not delete old voice logs at {path}: {e.Message}");
            }
        }
    }

    private Writer GetWriter(int round, Guid user, string username)
    {
        if (_writers.TryGetValue((round, user), out var writer))
            return writer;

        var directory = Path.Combine(_root!, round.ToString());
        Directory.CreateDirectory(directory);

        var stream = new FileStream(Path.Combine(directory, user + FileExtension), FileMode.Append, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete);
        writer = new Writer(stream);
        if (stream.Length == 0)
        {
            writer.Output.Write(Magic);
            writer.Output.Write(Version);
            writer.Output.Write(username);
        }

        _writers[(round, user)] = writer;
        return writer;
    }

    private void FlushWriters()
    {
        foreach (var writer in _writers.Values)
        {
            writer.Output.Flush();
        }
    }

    private byte MeasureLevel(byte[] payload)
    {
        if (!VoiceCodec.Decode(payload, _levelPcm))
            return 0;

        var energy = 0f;
        foreach (var sample in _levelPcm)
        {
            var value = sample / 32768f;
            energy += value * value;
        }

        var db = 10f * MathF.Log10(energy / _levelPcm.Length + 1e-12f);
        return (byte) Math.Clamp((db + 55f) / 45f * 255f, 0f, 255f);
    }

    private long ReadRoundStart(string directory)
    {
        var meta = Path.Combine(directory, RoundFile);
        if (File.Exists(meta) && long.TryParse(File.ReadAllText(meta).Trim(), out var start))
            return start;

        return new DateTimeOffset(Directory.GetCreationTimeUtc(directory)).ToUnixTimeMilliseconds();
    }

    private string? TrackPath(int round, Guid user)
    {
        return _root == null ? null : Path.Combine(_root, round.ToString(), user + FileExtension);
    }

    private TrackIndex GetIndex(int round, Guid user)
    {
        return _indices.GetOrAdd((round, user), _ => new TrackIndex());
    }

    private static void Parse(TrackIndex index, string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        if (stream.Length <= index.Parsed)
            return;

        stream.Position = index.Parsed;
        using var reader = new BinaryReader(stream);

        try
        {
            if (!index.HeaderRead)
            {
                var magic = reader.ReadBytes(Magic.Length);
                if (!magic.AsSpan().SequenceEqual(Magic) || reader.ReadByte() != Version)
                {
                    index.Parsed = stream.Length;
                    return;
                }

                index.Username = reader.ReadString();
                index.HeaderRead = true;
                index.Parsed = stream.Position;
            }

            while (stream.Position < stream.Length)
            {
                var type = reader.ReadByte();
                if (type == MetaRecord)
                {
                    var name = reader.ReadString();
                    var channel = reader.ReadString();
                    index.Name = name;
                    index.Channel = channel;
                }
                else if (type == FrameRecord)
                {
                    if (stream.Length - stream.Position < FrameRecordBytes - 1)
                        break;

                    var time = reader.ReadInt64();
                    var sequence = reader.ReadUInt16();
                    var flags = (VoiceLogFlags) reader.ReadByte();
                    var level = reader.ReadByte();
                    var offset = stream.Position;
                    stream.Position += VoiceCodec.FrameBytes;
                    AddFrame(index, time, sequence, flags, level, offset);
                }
                else
                {
                    index.Parsed = stream.Length;
                    return;
                }

                index.Parsed = stream.Position;
            }
        }
        catch (EndOfStreamException)
        {
        }
    }

    private static void AddFrame(TrackIndex index, long time, ushort sequence, VoiceLogFlags flags, byte level, long offset)
    {
        var segment = index.Segments.Count > 0 ? index.Segments[^1] : null;
        var slot = segment == null ? -1 : (ushort) (sequence - segment.FirstSequence);

        if (segment == null ||
            segment.Flags != flags ||
            segment.Name != index.Name ||
            segment.Channel != index.Channel ||
            time - segment.LastTimeMs > SegmentGapMs ||
            slot < segment.Offsets.Count ||
            slot - segment.Offsets.Count >= MaxSequenceGap ||
            slot >= MaxSegmentFrames)
        {
            segment = new SegmentIndex(time, sequence, flags, index.Name, index.Channel);
            index.Segments.Add(segment);
            slot = 0;
        }

        while (segment.Offsets.Count < slot)
        {
            segment.Offsets.Add(-1);
            segment.Levels.Add(0);
        }

        segment.Offsets.Add(offset);
        segment.Levels.Add(level);
        segment.LastTimeMs = time;
        index.TotalFrames++;
    }

    private sealed class Writer(FileStream stream) : IDisposable
    {
        public readonly BinaryWriter Output = new(stream);
        public string? Name;
        public string? Channel;

        public void Dispose()
        {
            Output.Dispose();
        }
    }

    private sealed class TrackIndex
    {
        public string Username = string.Empty;
        public long Parsed;
        public bool HeaderRead;
        public string Name = string.Empty;
        public string Channel = string.Empty;
        public long TotalFrames;
        public readonly List<SegmentIndex> Segments = new();
    }

    private sealed class SegmentIndex(long startMs, ushort firstSequence, VoiceLogFlags flags, string name, string channel)
    {
        public readonly long StartMs = startMs;
        public readonly ushort FirstSequence = firstSequence;
        public readonly VoiceLogFlags Flags = flags;
        public readonly string Name = name;
        public readonly string Channel = channel;
        public readonly List<long> Offsets = new();
        public readonly List<byte> Levels = new();
        public long LastTimeMs = startMs;
    }
}
