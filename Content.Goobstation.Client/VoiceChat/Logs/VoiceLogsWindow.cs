using System.Linq;
using System.Numerics;
using Content.Goobstation.Shared.VoiceChat;
using Robust.Client.Audio;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.VoiceChat.Logs;

public sealed class VoiceLogsWindow : DefaultWindow
{
    [Dependency] private readonly IResourceCache _cache = default!;
    [Dependency] private readonly IAudioManager _audio = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private const long LeadMs = 60;
    private const long PrefetchMs = 20000;
    private const float HeaderWidth = 170f;

    private static readonly VoicePlaybackParams PlaybackParams = new(Vector2.Zero, 1f, 0f, 1f, 10f, 1f, true, 1f);

    private readonly VoiceLogView _view = new();
    private readonly List<VoiceLogTrack> _tracks = new();
    private readonly Dictionary<Guid, Control> _rows = new();
    private readonly OptionButton _roundSelect;
    private readonly OptionButton _speakerSelect;
    private readonly Button _play;
    private readonly Label _time;
    private readonly Label _status;
    private readonly Label _empty;
    private readonly BoxContainer _trackList;
    private readonly HScrollBar _scroll;
    private readonly Font _font;

    private List<VoiceLogRound> _rounds = new();
    private List<VoiceLogSpeaker> _speakers = new();
    private int _roundId = -1;
    private long _roundStartMs;
    private bool _playing;
    private bool _updatingScroll;

    public event Action<int>? RoundSelected;
    public event Action? RefreshRequested;
    public event Action<int, Guid>? TrackRequested;
    public event Action<int, Guid, int, int>? AudioRequested;

    public VoiceLogsWindow()
    {
        IoCManager.InjectDependencies(this);

        Title = Loc.GetString("voice-logs-title");
        MinSize = new Vector2(960, 520);
        _font = new VectorFont(_cache.GetResource<FontResource>("/Fonts/NotoSans/NotoSans-Regular.ttf"), 9);

        _roundSelect = new OptionButton { MinWidth = 220 };
        _roundSelect.OnItemSelected += args =>
        {
            _roundSelect.SelectId(args.Id);
            RoundSelected?.Invoke(args.Id);
        };

        var refresh = new Button { Text = Loc.GetString("voice-logs-refresh") };
        refresh.OnPressed += _ => RefreshRequested?.Invoke();

        _speakerSelect = new OptionButton { MinWidth = 260 };
        _speakerSelect.OnItemSelected += args => _speakerSelect.SelectId(args.Id);

        var add = new Button { Text = Loc.GetString("voice-logs-add") };
        add.OnPressed += _ => AddSelectedSpeaker();

        var toolbar = new BoxContainer { Orientation = BoxContainer.LayoutOrientation.Horizontal, SeparationOverride = 6 };
        toolbar.AddChild(new Label { Text = Loc.GetString("voice-logs-round") });
        toolbar.AddChild(_roundSelect);
        toolbar.AddChild(refresh);
        toolbar.AddChild(new Control { HorizontalExpand = true });
        toolbar.AddChild(new Label { Text = Loc.GetString("voice-logs-player") });
        toolbar.AddChild(_speakerSelect);
        toolbar.AddChild(add);

        _play = new Button { Text = Loc.GetString("voice-logs-play"), MinWidth = 70 };
        _play.OnPressed += _ => SetPlaying(!_playing);

        _time = new Label { MinWidth = 150 };
        _status = new Label { FontColorOverride = Color.FromHex("#A9A9B3") };

        var zoomOut = new Button { Text = "−" };
        zoomOut.OnPressed += _ => ZoomCentered(1.5);
        var zoomIn = new Button { Text = "+" };
        zoomIn.OnPressed += _ => ZoomCentered(1 / 1.5);
        var fit = new Button { Text = Loc.GetString("voice-logs-fit") };
        fit.OnPressed += _ =>
        {
            _view.Fit();
            UpdateScroll();
        };

        var transport = new BoxContainer { Orientation = BoxContainer.LayoutOrientation.Horizontal, SeparationOverride = 6 };
        transport.AddChild(_play);
        transport.AddChild(_time);
        transport.AddChild(_status);
        transport.AddChild(new Control { HorizontalExpand = true });
        transport.AddChild(zoomOut);
        transport.AddChild(zoomIn);
        transport.AddChild(fit);

        var ruler = new VoiceLogRuler(_view, _font);
        ruler.SeekRequested += Seek;
        ruler.ViewChanged += UpdateScroll;

        var rulerRow = new BoxContainer { Orientation = BoxContainer.LayoutOrientation.Horizontal };
        rulerRow.AddChild(new Control { MinWidth = HeaderWidth });
        rulerRow.AddChild(ruler);

        _empty = new Label
        {
            Text = Loc.GetString("voice-logs-empty"),
            FontColorOverride = Color.FromHex("#A9A9B3"),
            Margin = new Thickness(8),
        };
        _trackList = new BoxContainer { Orientation = BoxContainer.LayoutOrientation.Vertical, SeparationOverride = 2 };
        _trackList.AddChild(_empty);

        var scrollContainer = new ScrollContainer { VerticalExpand = true, HScrollEnabled = false };
        scrollContainer.AddChild(_trackList);

        _scroll = new HScrollBar { HorizontalExpand = true };
        _scroll.OnValueChanged += _ =>
        {
            if (_updatingScroll)
                return;

            _view.StartMs = _scroll.Value;
            _view.Clamp();
        };

        var scrollRow = new BoxContainer { Orientation = BoxContainer.LayoutOrientation.Horizontal };
        scrollRow.AddChild(new Control { MinWidth = HeaderWidth });
        scrollRow.AddChild(_scroll);

        var root = new BoxContainer { Orientation = BoxContainer.LayoutOrientation.Vertical, SeparationOverride = 6 };
        root.AddChild(toolbar);
        root.AddChild(transport);
        root.AddChild(rulerRow);
        root.AddChild(scrollContainer);
        root.AddChild(scrollRow);
        Contents.AddChild(root);
    }

    public void SetState(VoiceLogsEuiState state)
    {
        _rounds = state.Rounds;
        _speakers = state.Speakers;

        _roundSelect.Clear();
        foreach (var round in _rounds)
        {
            var label = Loc.GetString(round.Current ? "voice-logs-round-current" : "voice-logs-round-entry",
                ("id", round.RoundId),
                ("length", VoiceLogView.Format(round.EndMs - round.StartMs)));
            _roundSelect.AddItem(label, round.RoundId);
        }

        if (_rounds.Any(round => round.RoundId == state.RoundId))
            _roundSelect.SelectId(state.RoundId);

        if (state.RoundId != _roundId)
        {
            ClearTracks();
            _roundId = state.RoundId;
            _roundStartMs = _rounds.FirstOrDefault(round => round.RoundId == state.RoundId)?.StartMs ?? 0;
            _view.PlayheadMs = 0;
        }

        _speakerSelect.Clear();
        for (var i = 0; i < _speakers.Count; i++)
        {
            var speaker = _speakers[i];
            _speakerSelect.AddItem($"{speaker.Username} · {VoiceLogView.Format((long) (speaker.TalkSeconds * 1000))}", i);
        }

        _status.Text = state.Loading ? Loc.GetString("voice-logs-loading") : string.Empty;
        UpdateTotal();
    }

    public void SetTrack(VoiceLogsTrackMessage message)
    {
        if (message.RoundId != _roundId)
            return;

        var track = _tracks.FirstOrDefault(entry => entry.UserId == message.UserId);
        var first = _tracks.Count == 0;
        if (track == null)
        {
            track = new VoiceLogTrack(message.UserId, message.Username);
            _tracks.Add(track);
            AddRow(track);
        }

        track.SetSegments(message.Segments, _roundStartMs);
        UpdateTotal();

        if (first)
        {
            _view.Fit();
            UpdateScroll();
        }
    }

    public void ReceiveAudio(VoiceLogsAudioMessage message)
    {
        if (message.RoundId != _roundId)
            return;

        var track = _tracks.FirstOrDefault(entry => entry.UserId == message.UserId);
        if (track == null)
            return;

        for (var i = 0; i < message.Payloads.Count; i++)
        {
            track.Audio[message.FirstSegment + i] = message.Payloads[i];
        }

        track.Requesting = false;
    }

    public IEnumerable<Guid> TrackUsers => _tracks.Select(track => track.UserId);

    public int RoundId => _roundId;

    public void StopAll()
    {
        SetPlaying(false);
        foreach (var track in _tracks)
        {
            StopStream(track);
        }
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if (_playing)
        {
            _view.PlayheadMs += (long) (args.DeltaSeconds * 1000f);
            if (_view.PlayheadMs >= _view.TotalMs)
            {
                _view.PlayheadMs = _view.TotalMs;
                SetPlaying(false);
            }

            FollowPlayhead();
        }

        foreach (var track in _tracks)
        {
            RequestAhead(track);
        }

        var now = _timing.RealTime;
        if (_playing)
            PushAudio(now);

        foreach (var track in _tracks)
        {
            track.Stream?.Update(now, PlaybackParams);
        }

        _time.Text = $"{VoiceLogView.Format(_view.PlayheadMs)} / {VoiceLogView.Format(_view.TotalMs)}";
    }

    private void AddSelectedSpeaker()
    {
        var index = _speakerSelect.SelectedId;
        if (index < 0 || index >= _speakers.Count)
            return;

        var speaker = _speakers[index];
        if (_tracks.All(track => track.UserId != speaker.UserId))
            TrackRequested?.Invoke(_roundId, speaker.UserId);
    }

    private void AddRow(VoiceLogTrack track)
    {
        _empty.Visible = false;

        var mute = new Button { Text = "M", ToggleMode = true, ToolTip = Loc.GetString("voice-logs-mute") };
        mute.OnToggled += args => track.Muted = args.Pressed;
        var solo = new Button { Text = "S", ToggleMode = true, ToolTip = Loc.GetString("voice-logs-solo") };
        solo.OnToggled += args => track.Solo = args.Pressed;
        var remove = new Button { Text = "✕", ToolTip = Loc.GetString("voice-logs-remove") };
        remove.OnPressed += _ => RemoveTrack(track);

        var buttons = new BoxContainer { Orientation = BoxContainer.LayoutOrientation.Horizontal, SeparationOverride = 2 };
        buttons.AddChild(mute);
        buttons.AddChild(solo);
        buttons.AddChild(remove);

        var header = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            MinWidth = HeaderWidth,
            MaxWidth = HeaderWidth,
            Margin = new Thickness(0, 0, 4, 0),
        };
        header.AddChild(new Label { Text = track.Username, ClipText = true });
        header.AddChild(buttons);

        var lane = new VoiceLogLane(_view, track, _font);
        lane.SeekRequested += Seek;
        lane.ViewChanged += UpdateScroll;

        var row = new BoxContainer { Orientation = BoxContainer.LayoutOrientation.Horizontal };
        row.AddChild(header);
        row.AddChild(lane);
        _trackList.AddChild(row);
        _rows[track.UserId] = row;
    }

    private void RemoveTrack(VoiceLogTrack track)
    {
        StopStream(track);
        _tracks.Remove(track);
        if (_rows.Remove(track.UserId, out var row))
            row.Orphan();

        _empty.Visible = _tracks.Count == 0;
        UpdateTotal();
    }

    private void ClearTracks()
    {
        foreach (var track in _tracks.ToList())
        {
            RemoveTrack(track);
        }
    }

    private void SetPlaying(bool playing)
    {
        _playing = playing;
        _play.Text = Loc.GetString(playing ? "voice-logs-pause" : "voice-logs-play");

        foreach (var track in _tracks)
        {
            StopStream(track);
            track.PushedUntil = _view.PlayheadMs;
        }
    }

    private void Seek(long ms)
    {
        _view.PlayheadMs = Math.Clamp(ms, 0, _view.TotalMs);
        foreach (var track in _tracks)
        {
            StopStream(track);
            track.PushedUntil = _view.PlayheadMs;
        }
    }

    private void RequestAhead(VoiceLogTrack track)
    {
        if (track.Requesting || track.Segments.Count == 0)
            return;

        var from = _view.PlayheadMs;
        var to = from + PrefetchMs;
        var first = -1;
        var last = -1;
        for (var i = 0; i < track.Segments.Count; i++)
        {
            if (track.SegmentEnd(i) < from)
                continue;

            if (track.Starts[i] > to)
                break;

            if (!track.Audio.ContainsKey(i))
            {
                if (first < 0)
                    first = i;

                last = i;
            }
            else if (first >= 0)
            {
                break;
            }
        }

        if (first < 0)
            return;

        track.Requesting = true;
        AudioRequested?.Invoke(_roundId, track.UserId, first, last);
    }

    private void PushAudio(TimeSpan now)
    {
        var anySolo = _tracks.Any(track => track.Solo);
        var until = _view.PlayheadMs + LeadMs;

        foreach (var track in _tracks)
        {
            var audible = anySolo ? track.Solo : !track.Muted;
            if (!audible)
            {
                StopStream(track);
                track.PushedUntil = until;
                continue;
            }

            if (until <= track.PushedUntil)
                continue;

            for (var i = 0; i < track.Segments.Count; i++)
            {
                var start = track.Starts[i];
                if (track.SegmentEnd(i) <= track.PushedUntil)
                    continue;

                if (start >= until)
                    break;

                if (!track.Audio.TryGetValue(i, out var payload))
                    continue;

                var frames = track.Segments[i].Levels.Length;
                var firstFrame = Math.Clamp((int) Math.Ceiling((track.PushedUntil - start) / (double) VoiceLogView.FrameMs), 0, frames);
                var endFrame = Math.Clamp((int) Math.Ceiling((until - start) / (double) VoiceLogView.FrameMs), 0, frames);
                for (var frame = firstFrame; frame < endFrame; frame++)
                {
                    var pcm = new short[VoiceCodec.FrameSamples];
                    if (!VoiceCodec.Decode(payload.AsSpan(frame * VoiceCodec.FrameBytes, VoiceCodec.FrameBytes), pcm))
                        continue;

                    track.Stream ??= new VoicePlaybackStream(_audio) { Global = true };
                    var flags = frame == 0 ? VoiceCodec.FlagTransmissionStart : (byte) 0;
                    track.Stream.AddFrame(track.Sequence++, flags, pcm, now);
                }
            }

            track.PushedUntil = until;
        }
    }

    private static void StopStream(VoiceLogTrack track)
    {
        track.Stream?.Dispose();
        track.Stream = null;
    }

    private void FollowPlayhead()
    {
        if (_view.PlayheadMs >= _view.StartMs && _view.PlayheadMs <= _view.EndMs - _view.DurationMs * 0.05)
            return;

        _view.StartMs = _view.PlayheadMs - _view.DurationMs * 0.1;
        _view.Clamp();
        UpdateScroll();
    }

    private void ZoomCentered(double factor)
    {
        _view.Zoom(_view.StartMs + _view.DurationMs / 2, factor);
        UpdateScroll();
    }

    private void UpdateTotal()
    {
        var round = _rounds.FirstOrDefault(entry => entry.RoundId == _roundId);
        var total = round == null ? 0 : round.EndMs - round.StartMs;
        if (round is { Current: true })
            total = Math.Max(total, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - round.StartMs);

        foreach (var track in _tracks)
        {
            total = Math.Max(total, track.EndMs + 1000);
        }

        _view.TotalMs = Math.Max(total, (long) VoiceLogView.MinDurationMs);
        _view.Clamp();
        UpdateScroll();
    }

    private void UpdateScroll()
    {
        _updatingScroll = true;
        _scroll.MinValue = 0;
        _scroll.MaxValue = _view.TotalMs;
        _scroll.Page = (float) _view.DurationMs;
        _scroll.Value = (float) _view.StartMs;
        _updatingScroll = false;
    }
}
