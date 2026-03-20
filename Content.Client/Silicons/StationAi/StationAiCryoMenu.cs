using System.Numerics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using static Robust.Client.UserInterface.Controls.BoxContainer;

namespace Content.Client.Silicons.StationAi;
public sealed class StationAiCryoMenu : DefaultWindow
{
    public readonly Button DenyButton;
    public readonly Button ConfirmButton;

    public StationAiCryoMenu()
    {
        Title = Loc.GetString("station-ai-cryo-menu-title");
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
                            Text = Loc.GetString("station-ai-cryo-menu-text")
                        }),
                        new BoxContainer
                        {
                            Orientation = LayoutOrientation.Horizontal,
                            Align = AlignMode.Center,
                            Children =
                            {
                                (ConfirmButton = new Button
                                {
                                    Text = Loc.GetString("station-ai-cryo-menu-confirm")
                                }),

                                (new Control()
                                {
                                    MinSize = new Vector2(20, 0)
                                }),

                                (DenyButton = new Button
                                {
                                    Text = Loc.GetString("station-ai-cryo-menu-deny")
                                })
                            }
                        }
                    }
                }
            }
        });
    }
}
