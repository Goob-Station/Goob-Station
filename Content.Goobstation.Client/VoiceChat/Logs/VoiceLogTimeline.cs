using System.Numerics;
using Content.Goobstation.Shared.VoiceChat;
using Content.Shared.Radio;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Shared.Input;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.VoiceChat.Logs;

public sealed class VoiceLogView
{
    public const double MinDurationMs = 2000;
    public const int FrameMs = VoiceCodec.FrameSamples * 1000 / VoiceCodec.SampleRate;

    public long TotalMs = 60000;
    public double StartMs;
    public double DurationMs = 60000;
    public long PlayheadMs;

    public double EndMs => StartMs + DurationMs;

    public void Fit()
    {
        StartMs = 0;
        DurationMs = Math.Max(TotalMs, MinDurationMs);
    }

    public void Zoom(double anchorMs, double factor)
    {
        var ratio = (anchorMs - StartMs) / DurationMs;
        DurationMs = Math.Clamp(DurationMs * factor, MinDurationMs, Math.Max(TotalMs, MinDurationMs));
        StartMs = anchorMs - ratio * DurationMs;
        Clamp();
    }

    public void Clamp()
    {
        DurationMs = Math.Clamp(DurationMs, MinDurationMs, Math.Max(TotalMs, MinDurationMs));
        StartMs = Math.Clamp(StartMs, 0, Math.Max(0, TotalMs - DurationMs));
    }

    public double ToMs(float x, float width)
    {
        return StartMs + x / Math.Max(width, 1f) * DurationMs;
    }

    public float ToX(double ms, float width)
    {
        return (float) ((ms - StartMs) / DurationMs * width);
    }

    public static string Format(long ms)
    {
        var time = TimeSpan.FromMilliseconds(Math.Max(0, ms));
        return time.TotalHours >= 1
            ? $"{(int) time.TotalHours}:{time.Minutes:00}:{time.Seconds:00}"
            : $"{time.Minutes:00}:{time.Seconds:00}.{time.Milliseconds / 100}";
    }
}

public sealed class VoiceLogTrack(Guid userId, string username)
{
    public readonly Guid UserId = userId;
    public string Username = username;
    public List<VoiceLogSegment> Segments = new();
    public long[] Starts = Array.Empty<long>();
    public readonly Dictionary<int, byte[]> Audio = new();
    public bool Requesting;
    public bool Muted;
    public bool Solo;
    public VoicePlaybackStream? Stream;
    public long PushedUntil;
    public ushort Sequence;

    public long EndMs => Segments.Count == 0 ? 0 : Starts[^1] + Segments[^1].Levels.Length * VoiceLogView.FrameMs;

    public void SetSegments(List<VoiceLogSegment> segments, long roundStartMs)
    {
        var previous = Segments.Count;
        Segments = segments;
        Starts = new long[segments.Count];
        for (var i = 0; i < segments.Count; i++)
        {
            Starts[i] = segments[i].StartMs - roundStartMs;
        }

        if (previous > 0)
            Audio.Remove(previous - 1);
    }

    public long SegmentEnd(int index)
    {
        return Starts[index] + Segments[index].Levels.Length * VoiceLogView.FrameMs;
    }
}

public abstract class VoiceLogTimelineControl : Control
{
    protected readonly VoiceLogView View;
    private bool _dragging;

    public event Action<long>? SeekRequested;
    public event Action? ViewChanged;

    protected VoiceLogTimelineControl(VoiceLogView view)
    {
        View = view;
        MouseFilter = MouseFilterMode.Stop;
        HorizontalExpand = true;
    }

    protected override void KeyBindDown(GUIBoundKeyEventArgs args)
    {
        base.KeyBindDown(args);

        if (args.Function != EngineKeyFunctions.UIClick)
            return;

        _dragging = true;
        Seek(args.RelativePosition.X);
        args.Handle();
    }

    protected override void KeyBindUp(GUIBoundKeyEventArgs args)
    {
        base.KeyBindUp(args);

        if (args.Function == EngineKeyFunctions.UIClick)
            _dragging = false;
    }

    protected override void MouseMove(GUIMouseMoveEventArgs args)
    {
        base.MouseMove(args);

        if (_dragging)
            Seek(args.RelativePosition.X);
    }

    protected override void MouseExited()
    {
        base.MouseExited();
        _dragging = false;
    }

    protected override void MouseWheel(GUIMouseWheelEventArgs args)
    {
        base.MouseWheel(args);

        var anchor = View.ToMs(args.RelativePosition.X, Size.X);
        View.Zoom(anchor, args.Delta.Y > 0 ? 0.8 : 1.25);
        ViewChanged?.Invoke();
        args.Handle();
    }

    protected void DrawPlayhead(DrawingHandleScreen handle)
    {
        var x = View.ToX(View.PlayheadMs, PixelWidth);
        if (x >= 0 && x <= PixelWidth)
            handle.DrawRect(new UIBox2(x - 1f, 0f, x + 1f, PixelHeight), Color.FromHex("#E8E8F0"));
    }

    private void Seek(float x)
    {
        SeekRequested?.Invoke((long) Math.Clamp(View.ToMs(x, Size.X), 0, View.TotalMs));
    }
}

public sealed class VoiceLogRuler : VoiceLogTimelineControl
{
    private static readonly long[] Steps = { 1000, 5000, 10000, 30000, 60000, 300000, 600000, 1800000, 3600000 };
    private readonly Font _font;

    public VoiceLogRuler(VoiceLogView view, Font font) : base(view)
    {
        _font = font;
        MinHeight = 22;
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        base.Draw(handle);

        handle.DrawRect(new UIBox2(0f, 0f, PixelWidth, PixelHeight), Color.FromHex("#202128"));

        var step = Steps[^1];
        foreach (var candidate in Steps)
        {
            if (View.ToX(View.StartMs + candidate, PixelWidth) - View.ToX(View.StartMs, PixelWidth) >= 80f * UIScale)
            {
                step = candidate;
                break;
            }
        }

        for (var tick = (long) (View.StartMs / step) * step; tick <= View.EndMs; tick += step)
        {
            var x = View.ToX(tick, PixelWidth);
            if (x < 0)
                continue;

            handle.DrawRect(new UIBox2(x, PixelHeight * 0.55f, x + 1f, PixelHeight), Color.FromHex("#6A6C7A"));
            handle.DrawString(_font, new Vector2(x + 3f, 1f), VoiceLogView.Format(tick), Color.FromHex("#A9A9B3"));
        }

        DrawPlayhead(handle);
    }
}

public sealed class VoiceLogLane : VoiceLogTimelineControl
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    private static readonly Color Background = Color.FromHex("#15161B");
    private static readonly Color LocalColor = Color.FromHex("#9FD8A0");
    private static readonly Color LobbyColor = Color.FromHex("#B69CFF");
    private static readonly Color BroadcastColor = Color.FromHex("#FFC844");
    private static readonly Color BlockedColor = Color.FromHex("#E04545");
    private static readonly Color RadioFallback = Color.FromHex("#2CDB2C");
    private static readonly Color GodColor = Color.FromHex("#FFE9A8");

    private readonly VoiceLogTrack _track;
    private readonly Font _font;

    public VoiceLogLane(VoiceLogView view, VoiceLogTrack track, Font font) : base(view)
    {
        IoCManager.InjectDependencies(this);
        _track = track;
        _font = font;
        MinHeight = 52;
        RectClipContent = true;
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        base.Draw(handle);

        var width = (float) PixelWidth;
        var height = (float) PixelHeight;
        var middle = height * 0.55f;
        var amplitude = height * 0.4f;
        handle.DrawRect(new UIBox2(0f, 0f, width, height), Background);

        var msPerPixel = View.DurationMs / Math.Max(width, 1f);
        for (var i = 0; i < _track.Segments.Count; i++)
        {
            var segment = _track.Segments[i];
            var start = _track.Starts[i];
            var end = _track.SegmentEnd(i);
            if (end < View.StartMs || start > View.EndMs)
                continue;

            var color = SegmentColor(segment);
            var x0 = Math.Max(0f, View.ToX(start, width));
            var x1 = Math.Min(width, Math.Max(View.ToX(end, width), x0 + 1f));
            handle.DrawRect(new UIBox2(x0, 0f, x1, height), color.WithAlpha(_track.Audio.ContainsKey(i) ? 0.22f : 0.12f));

            for (var x = (int) x0; x < x1; x++)
            {
                var from = (int) ((View.StartMs + x * msPerPixel - start) / VoiceLogView.FrameMs);
                var to = (int) Math.Ceiling((View.StartMs + (x + 1) * msPerPixel - start) / VoiceLogView.FrameMs);
                from = Math.Clamp(from, 0, segment.Levels.Length - 1);
                to = Math.Clamp(to, from + 1, segment.Levels.Length);

                var peak = 0;
                for (var frame = from; frame < to; frame++)
                {
                    peak = Math.Max(peak, segment.Levels[frame]);
                }

                var half = MathF.Max(0.5f, peak / 255f * amplitude);
                handle.DrawRect(new UIBox2(x, middle - half, x + 1f, middle + half), color);
            }

            if (x1 - x0 > 60f * UIScale)
                handle.DrawString(_font, new Vector2(x0 + 3f, 1f), SegmentLabel(segment), color);
        }

        DrawPlayhead(handle);
    }

    private Color SegmentColor(VoiceLogSegment segment)
    {
        if ((segment.Flags & VoiceLogFlags.God) != 0)
            return GodColor;

        if ((segment.Flags & VoiceLogFlags.Blocked) != 0)
            return BlockedColor;

        if ((segment.Flags & VoiceLogFlags.Lobby) != 0)
            return LobbyColor;

        if ((segment.Flags & VoiceLogFlags.Broadcast) != 0)
            return BroadcastColor;

        if ((segment.Flags & VoiceLogFlags.Radio) != 0)
        {
            return segment.Channel.Length > 0 && _prototype.TryIndex((ProtoId<RadioChannelPrototype>) segment.Channel, out var channel)
                ? channel.Color
                : RadioFallback;
        }

        return LocalColor;
    }

    private string SegmentLabel(VoiceLogSegment segment)
    {
        var name = segment.Name.Length > 0 ? segment.Name : Loc.GetString("voice-logs-lobby");
        if ((segment.Flags & VoiceLogFlags.Blocked) != 0)
            return Loc.GetString("voice-logs-blocked", ("name", name));

        if ((segment.Flags & VoiceLogFlags.Radio) != 0 &&
            segment.Channel.Length > 0 &&
            _prototype.TryIndex((ProtoId<RadioChannelPrototype>) segment.Channel, out var channel))
        {
            return $"{name} · {channel.LocalizedName}";
        }

        return name;
    }
}
