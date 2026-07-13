// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.UserInterface.Controls;

public sealed class HLine : PanelContainer
{
    public Color? Color
    {
        get
        {
            StyleBox box;
            if (PanelOverride != null)
                box = PanelOverride;
            if (TryGetStyleProperty<StyleBox>(StylePropertyPanel, out var _box))
                box = _box;
            else
                return null;

            if (box is StyleBoxFlat boxFlat)
                return boxFlat.BackgroundColor;
            return null;
        }
        set =>
            // should use style classes instead in ui code but keeping this functionality for consistency
            PanelOverride = new StyleBoxFlat() { BackgroundColor = value!.Value };
    }

    public float? Thickness
    {
        get => MinHeight;
        set => MinHeight = value!.Value;
    }

    public HLine()
    {
    }
}
