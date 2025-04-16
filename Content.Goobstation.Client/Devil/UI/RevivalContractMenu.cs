// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using static Robust.Client.UserInterface.Controls.BoxContainer;

namespace Content.Goobstation.Client.Devil.UI;

public sealed class RevivalContractMenu : DefaultWindow
{
    public readonly Button DenyButton;
    public readonly Button AcceptButton;

    public RevivalContractMenu()
    {
        Title = Loc.GetString("revival-contract-menu-title");

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
                            Text = Loc.GetString("revival-contract-menu-text")
                        }),
                        new BoxContainer
                        {
                            Orientation = LayoutOrientation.Horizontal,
                            Align = AlignMode.Center,
                            Children =
                            {
                                (AcceptButton = new Button
                                {
                                    Text = Loc.GetString("accept-cloning-window-accept-button"),
                                }),

                                (new Control()
                                {
                                    MinSize = new Vector2(20, 0)
                                }),

                                (DenyButton = new Button
                                {
                                    Text = Loc.GetString("accept-cloning-window-deny-button"),
                                })
                            }
                        },
                    }
                },
            }
        });
    }
}
