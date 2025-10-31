// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.RichText;
using Robust.Shared.Utility;

namespace Content.Goobstation.UIKit.UserInterface.RichText;

public sealed class EntityTextureTag : BaseTextureTag, IMarkupTagHandler
{
    public string Name => "enttex";

    public bool TryCreateControl(MarkupNode node, [NotNullWhen(true)] out Control? control)
    {
        control = null;

        if (!node.Attributes.TryGetValue("id", out var idParameter) || !idParameter.TryGetLong(out var id))
            return false;

        if (!node.Attributes.TryGetValue("size", out var size) || !size.TryGetLong(out var sizeValue))
        {
            sizeValue = 32;
        }

        if (!TryDrawIconEntity(new NetEntity((int) id), sizeValue.Value, out var texture))
            return false;

        control = texture;

        return true;
    }
}
