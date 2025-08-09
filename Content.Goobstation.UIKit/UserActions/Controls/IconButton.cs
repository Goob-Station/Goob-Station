// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using static Robust.Client.UserInterface.Controls.BaseButton;
using static Robust.Client.UserInterface.Controls.BoxContainer;

namespace Content.Goobstation.UIKit.UserActions.Controls;

[Virtual]
public class IconButton : Button
{
    private readonly BoxContainer _mainContainer;

    public readonly TextureRect Icon;
    public readonly RichTextLabel Label;
    //public readonly PanelContainer HighlightRect;

    public IconButton(string name)
    {
        MinSize = new Vector2(0, 24);
        Margin = new Thickness(1);
        HorizontalAlignment = HAlignment.Left;

        _mainContainer = new BoxContainer
        {
            Orientation = LayoutOrientation.Horizontal,
            //HorizontalExpand = true,
            MinSize = new Vector2(0, 24),
            Margin = new Thickness(1)
        };
        AddChild(_mainContainer);

        Icon = new TextureRect
        {
            HorizontalExpand = true,
            VerticalExpand = true,
            HorizontalAlignment = HAlignment.Left,
            VerticalAlignment = VAlignment.Center,
            Stretch = TextureRect.StretchMode.Scale,
            Margin = new Thickness(0, 0, 5, 0),
            TextureScale = new Vector2(1, 1),
            MinSize = new Vector2(24, 24),
            MaxSize = new Vector2(24, 24),
            Visible = true
        };
        _mainContainer.AddChild(Icon);

        Label = new RichTextLabel
        {
            HorizontalExpand = true,
            VerticalExpand = true,
            HorizontalAlignment = HAlignment.Left,
            VerticalAlignment = VAlignment.Center,
            Margin = new Thickness(1),
            Text = name,
            Visible = true
        };
        _mainContainer.AddChild(Label);
    }

    protected override void MouseExited()
    {
        base.MouseExited();
    }

    protected override void MouseEntered()
    {
        base.MouseEntered();
    }
}
