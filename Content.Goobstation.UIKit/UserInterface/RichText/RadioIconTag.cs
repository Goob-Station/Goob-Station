// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 starch <starchpersonal@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Configuration;
using Robust.Shared.Utility;

namespace Content.Goobstation.UIKit.UserInterface.RichText;

public sealed class RadioIconTag : BaseTextureTag
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IResourceCache _cache = default!;

    public override string Name => "radicon";

    public override bool TryGetControl(MarkupNode node, [NotNullWhen(true)] out Control? control)
    {
        control = null;

        if (!node.Attributes.TryGetValue("text", out var text))
            return false;

        if (!node.Attributes.TryGetValue("color", out var color))
            return false;

        control = DrawText(text.ToString(), color.ToString());

        return true;
    }

    private Control DrawText(string text, string color)
    {
        var label = new Label();

        color = ClearString(color);
        text = ClearString(text);

        label.Text = text;
        label.FontColorOverride = Color.FromHex(color);
        label.FontOverride = new VectorFont(_cache.GetResource<FontResource>("/Fonts/Minecraft/Minecrafter_3.ttf"), 13);

        return label;
    }

}
