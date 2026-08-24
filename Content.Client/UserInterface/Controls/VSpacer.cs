// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Client.UserInterface;

namespace Content.Client.UserInterface.Controls;

public sealed class VSpacer : Control
{
    public float Spacing{ get => MinWidth; set => MinWidth = value; }
    public VSpacer()
    {
        MinWidth = Spacing;
    }
    public VSpacer(float width = 5)
    {
        Spacing = width;
        MinWidth = width;
    }
}