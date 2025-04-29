// SPDX-FileCopyrightText: 2022 Jesse Rougeau <jmaster9999@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.UserInterface.Controls;

public sealed class HLine : Container
{
    public Color? Color
    {
        get
        {
            if (_line.PanelOverride is StyleBoxFlat styleBox) return styleBox.BackgroundColor;
            return null;
        }
        set
        {
            if (_line.PanelOverride is StyleBoxFlat styleBox) styleBox.BackgroundColor = value!.Value;
        }
    }

    public float? Thickness {
        get
        {
            if (_line.PanelOverride is StyleBoxFlat styleBox) return styleBox.ContentMarginTopOverride;
            return null;
        }
        set
        {
            if (_line.PanelOverride is StyleBoxFlat styleBox) styleBox.ContentMarginTopOverride = value!.Value;
        }
    }

    private readonly PanelContainer _line;

    public HLine()
    {
        _line = new PanelContainer();
        _line.PanelOverride = new StyleBoxFlat();
        _line.PanelOverride.ContentMarginTopOverride = Thickness;
        AddChild(_line);
    }

}