// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 moonheart08 <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.Guidebook.Richtext;

public sealed class Box : BoxContainer, IDocumentTag
{
    public bool TryParseTag(Dictionary<string, string> args, [NotNullWhen(true)] out Control? control)
    {
        HorizontalExpand = true;
        control = this;

        if (args.TryGetValue("Margin", out var margin))
            Margin = new Thickness(float.Parse(margin));

        if (args.TryGetValue("Orientation", out var orientation))
            Orientation = Enum.Parse<LayoutOrientation>(orientation);
        else
            Orientation = LayoutOrientation.Horizontal;

        if (args.TryGetValue("HorizontalAlignment", out var halign))
            HorizontalAlignment = Enum.Parse<HAlignment>(halign);
        else
            HorizontalAlignment = HAlignment.Center;

        if (args.TryGetValue("VerticalAlignment", out var valign))
            VerticalAlignment = Enum.Parse<VAlignment>(valign);

        return true;
    }
}