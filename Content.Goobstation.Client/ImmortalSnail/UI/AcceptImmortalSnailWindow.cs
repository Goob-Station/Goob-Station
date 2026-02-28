using System.Numerics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using static Robust.Client.UserInterface.Controls.BoxContainer;

namespace Content.Goobstation.Client.ImmortalSnail.UI;

public sealed class AcceptImmortalSnailWindow : DefaultWindow
{
    [Dependency] private readonly IGameTiming _timing = default!;

    public readonly Button DenyButton;
    public readonly Button AcceptButton;
    public readonly Label TimerLabel;

    private TimeSpan _endTime;

    public AcceptImmortalSnailWindow()
    {
        IoCManager.InjectDependencies(this);

        Title = Loc.GetString("immortal-snail-offer-title");

        MinSize = new Vector2(420, 300);

        var messageLabel = new RichTextLabel
        {
            HorizontalAlignment = HAlignment.Left,
            MaxWidth = 400,
        };
        messageLabel.SetMessage(FormattedMessage.FromMarkupPermissive(Loc.GetString("immortal-snail-offer-message")));

        Contents.AddChild(new BoxContainer
        {
            Orientation = LayoutOrientation.Vertical,
            Children =
            {
                messageLabel,
                new Control
                {
                    MinSize = new Vector2(0, 15)
                },
                (TimerLabel = new Label
                {
                    Text = "",
                    HorizontalAlignment = HAlignment.Center,
                }),
                new Control
                {
                    MinSize = new Vector2(0, 15)
                },
                new BoxContainer
                {
                    Orientation = LayoutOrientation.Horizontal,
                    Align = AlignMode.Center,
                    Children =
                    {
                        (AcceptButton = new Button
                        {
                            Text = Loc.GetString("immortal-snail-offer-accept-button"),
                        }),

                        new Control
                        {
                            MinSize = new Vector2(20, 0)
                        },

                        (DenyButton = new Button
                        {
                            Text = Loc.GetString("immortal-snail-offer-deny-button"),
                        })
                    }
                },
            }
        });
    }

    public void SetEndTime(TimeSpan endTime)
    {
        _endTime = endTime;
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        var timeLeft = _endTime - _timing.CurTime;

        var seconds = (int) Math.Ceiling(timeLeft.TotalSeconds);
        TimerLabel.Text = Loc.GetString("immortal-snail-offer-timer", ("seconds", seconds));
    }
}
