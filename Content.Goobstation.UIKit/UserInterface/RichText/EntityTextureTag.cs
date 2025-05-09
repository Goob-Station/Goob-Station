// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.UIKit.UserInterface.RichText;

public sealed class EntityTextureTag : BaseTextureTag
{
    public override string Name => "enttex";

    public override bool TryGetControl(MarkupNode node, [NotNullWhen(true)] out Control? control)
    {
        control = null;

        if (!node.Attributes.TryGetValue("id", out var entProtoId))
            return false;

        if (!node.Attributes.TryGetValue("size", out var size) || !size.TryGetLong(out var sizeValue))
        {
            sizeValue = 32;
        }

        if (!TryDrawIconEntity((EntProtoId) entProtoId.ToString(), sizeValue.Value, out var texture))
            return false;

        control = texture;

        return true;
    }
}
