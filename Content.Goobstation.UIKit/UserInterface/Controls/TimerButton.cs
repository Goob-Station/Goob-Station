using Robust.Client.UserInterface.Controls;
using Robust.Shared.Timing;

namespace Content.Goobstation.UIKit.UserInterface.Controls;

public sealed class TimerButton : Button
{
    [Dependency] private readonly IGameTiming _timing = default!;

    private readonly string _label;
    private readonly TimeSpan _timeSpan;
    private readonly TimeSpan _startTime;

    public TimerButton(string label, TimeSpan time)
    {
        IoCManager.InjectDependencies(this);
        _label = label;
        _timeSpan = time;
        _startTime = _timing.RealTime;
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if (_timeSpan == TimeSpan.Zero)
            return;

        if (Disabled)
        {
            Text = _label;
            return;
        }

        var text = _label;

        var cur = _timing.RealTime;
        var end = _startTime + _timeSpan;
        if (cur > end)
        {
            Disabled = true;
            Text = text;
            return;
        }

        var display = (end - cur).ToString(@"ss\:ms");
        Text = text != string.Empty ? $"{text} ({display})" : display;
    }
}
