using System.Numerics;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using static Robust.Client.UserInterface.Controls.BoxContainer;

namespace Content.Goobstation.Client.Gangwars;

public sealed class GangInviteWindow : DefaultWindow
{
    public event Action? OnAccepted;
    public event Action? OnDenied;

    [Dependency] private readonly IGameTiming _timing = default!;

    private readonly Label _timerLabel;
    public TimeSpan EndTime { get; private set; }
    private bool _responded;

    public GangInviteWindow(string leaderName, Color gangColor, string gangName, float seconds = 10f)
    {
        IoCManager.InjectDependencies(this);
        EndTime = _timing.CurTime + TimeSpan.FromSeconds(seconds);

        Title = Loc.GetString("gang-invite-window-title");
        MinSize = new Vector2(300, 130);

        _timerLabel = new Label
        {
            HorizontalAlignment = HAlignment.Center,
            Text = Loc.GetString("gang-invite-window-timer", ("seconds", (int) seconds)),
        };

        var colorHex = gangColor.ToHexNoAlpha().TrimStart('#');
        var promptLabel = new RichTextLabel
        {
            HorizontalAlignment = HAlignment.Center,
        };
        promptLabel.SetMessage(FormattedMessage.FromMarkupOrThrow(Loc.GetString("gang-invite-window-prompt", ("leader", leaderName), ("color", colorHex), ("gangName", gangName))));

        var promptBox = new BoxContainer
        {
            Orientation = LayoutOrientation.Horizontal,
            HorizontalAlignment = HAlignment.Center,
            Children = { promptLabel },
        };

        var acceptButton = new Button
        {
            Text = Loc.GetString("gang-invite-window-accept")
        };
        var denyButton = new Button
        {
            Text = Loc.GetString("gang-invite-window-deny")
        };

        acceptButton.OnPressed += _ =>
        {
            if (_responded)
                return;
            _responded = true;
            OnAccepted?.Invoke();
        };

        denyButton.OnPressed += _ =>
        {
            if (_responded)
                return;
            _responded = true;
            OnDenied?.Invoke();
        };

        Contents.AddChild(new BoxContainer
        {
            Orientation = LayoutOrientation.Vertical,
            SeparationOverride = 8,
            Children =
            {
                promptBox,
                _timerLabel,
                new BoxContainer
                {
                    Orientation = LayoutOrientation.Horizontal,
                    Align = AlignMode.Center,
                    SeparationOverride = 8,
                    Children = { acceptButton, denyButton },
                },
            },
        });
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if (_responded)
            return;

        var remaining = EndTime - _timing.CurTime;
        var secs = Math.Max(0, (int) Math.Ceiling(remaining.TotalSeconds));
        _timerLabel.Text = Loc.GetString("gang-invite-window-timer", ("seconds", secs));
    }

    public bool TryTimedOut()
    {
        if (_responded || _timing.CurTime < EndTime)
            return false;

        _responded = true;
        return true;
    }
}
