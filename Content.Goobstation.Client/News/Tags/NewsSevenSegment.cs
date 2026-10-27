using System.Diagnostics.CodeAnalysis;
using Content.Client.Guidebook.Richtext;
using Content.Goobstation.UIKit.UserInterface.Controls;
using Content.Goobstation.UIKit.UserInterface.Lobby;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.News.Tags;

public sealed partial class NewsSevenSegment : PanelContainer, IDocumentTag
{
    [Dependency] private IGameTiming _timing = default!;

    private readonly SevenSegmentDisplay _display = new() { HorizontalAlignment = HAlignment.Center, Margin = new Thickness(0, 2, 0, 0) };
    private readonly Label _caption = new() { HorizontalAlignment = HAlignment.Center, StyleClasses = { HullStyleClasses.LcdCaption } };

    private TimeSpan? _countdown;
    private TimeSpan _started;

    public NewsSevenSegment()
    {
        IoCManager.InjectDependencies(this);
        StyleClasses.Add(HullStyleClasses.Lcd);

        var box = new BoxContainer { Orientation = BoxContainer.LayoutOrientation.Vertical };
        box.AddChild(_caption);
        box.AddChild(_display);
        AddChild(box);
    }

    public bool TryParseTag(Dictionary<string, string> args, [NotNullWhen(true)] out Control? control)
    {
        HorizontalAlignment = NewsTagArgs.HAlign(args, HAlignment.Center);
        Margin = NewsTagArgs.Margin(args);

        _caption.Visible = args.TryGetValue("Caption", out var caption);
        if (caption != null)
            _caption.Text = Loc.GetString(caption);

        if (args.TryGetValue("MinDigits", out var minDigits) && int.TryParse(minDigits, out var digits))
            _display.MinDigits = digits;

        if (args.TryGetValue("Countdown", out var countdown) && int.TryParse(countdown, out var seconds) && seconds > 0)
        {
            _countdown = TimeSpan.FromSeconds(seconds);
            _started = _timing.RealTime;
        }

        _display.Text = args.GetValueOrDefault("Text") ?? string.Empty;

        control = this;
        return true;
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if (_countdown is not { } countdown)
            return;

        var elapsed = (_timing.RealTime - _started).Ticks % countdown.Ticks;
        var left = countdown - TimeSpan.FromTicks(elapsed);
        _display.Text = $"{(int) left.TotalMinutes}:{left.Seconds:D2}";
    }
}
