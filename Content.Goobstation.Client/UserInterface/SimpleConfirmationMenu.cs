using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Client.UserInterface;
using System.Numerics;

using static Robust.Client.UserInterface.Controls.BoxContainer;
using Content.Client.UserInterface.Controls;

namespace Content.Goobstation.Client.UserInterface;

public sealed class SimpleConfirmationMenu : DefaultWindow
{
    public readonly ConfirmButton ConfirmButton;
    public readonly Button CancelButton;

    public SimpleConfirmationMenu(string locTitle, string locConfirm, string locCancel)
    {
        Title = Loc.GetString("ui-confirmation-title");

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
                        new Label()
                        {
                            Text = Loc.GetString(locTitle)
                        },
                        new BoxContainer
                        {
                            Orientation = LayoutOrientation.Horizontal,
                            Align = AlignMode.Center,
                            Children =
                            {
                                (ConfirmButton = new ConfirmButton
                                {
                                    Text = Loc.GetString(locConfirm),
                                }),

                                // separator
                                new Control() { MinSize = new Vector2(20, 0) },

                                (CancelButton = new Button { Text = Loc.GetString(locCancel), })
                            }
                        },
                    }
                },
            }
        });
    }
}
