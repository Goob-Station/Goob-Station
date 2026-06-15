using System.Numerics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using static Robust.Client.UserInterface.Controls.BoxContainer;

namespace Content.Goobstation.Client.Silicon;

public sealed class StationAiEarlyLeaveMenu : DefaultWindow
{
    public readonly Button DenyButton;
    public readonly Button ConfirmButton;

    public StationAiEarlyLeaveMenu()
    {
        Title = Loc.GetString("station-ai-earlyleave-menu-title");
        Contents.AddChild(new BoxContainer
        {
            Orientation = LayoutOrientation.Vertical,
            Children =
            {
                new BoxContainer
                {
                    Orientation = LayoutOrientation.Vertical,
                    Children =
                    {
                        (new Label()
                        {
                            Text = Loc.GetString("station-ai-earlyleave-menu-text")
                        }),
                        new BoxContainer
                        {
                            Orientation = LayoutOrientation.Horizontal,
                            Align = AlignMode.Center,
                            Children =
                            {
                                (ConfirmButton = new Button
                                {
                                    Text = Loc.GetString("station-ai-earlyleave-menu-confirm")
                                }),

                                (new Control()
                                {
                                    MinSize = new Vector2(20, 0)
                                }),

                                (DenyButton = new Button
                                {
                                    Text = Loc.GetString("station-ai-earlyleave-menu-deny")
                                })
                            }
                        }
                    }
                }
            }
        });
    }
}
