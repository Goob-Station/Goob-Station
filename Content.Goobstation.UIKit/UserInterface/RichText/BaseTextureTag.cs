// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Content.Goobstation.UIKit.UserInterface.Controls;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.UIKit.UserInterface.RichText;

public abstract class BaseTextureTag
{
    [Dependency] protected readonly IEntitySystemManager EntitySystemManager = default!;

    protected static bool TryDrawIcon(Texture tex,
        long scaleValue,
        Vector2 offset,
        string? tooltip,
        [NotNullWhen(true)] out Control? control)
    {
        var texture = new TooltipTextureRect(tooltip, offset);

        texture.Texture = tex;
        texture.TextureScale = new Vector2(scaleValue, scaleValue);

        control = texture;
        return true;
    }

    protected static bool TryDrawIconEntity(NetEntity netEntity, long spriteSize, [NotNullWhen(true)] out Control? control)
    {
        control = null;
        var spriteView = new StaticSpriteView()
        {
            OverrideDirection = Direction.South,
            SetSize = new Vector2(spriteSize * 2, spriteSize * 2),
        };

        spriteView.SetEntity(netEntity);
        spriteView.Scale = new Vector2(2, 2);

        control = spriteView;
        return true;
    }

    /// <summary>
    /// Очищает строку от мусора, который приходит вместе с ней
    /// </summary>
    /// <remarks>
    /// Почему мне приходят строки в говне
    /// </remarks>
    protected static string ClearString(string str)
    {
        str = str.Replace("=", "");
        str = str.Replace("\"", "");
        str = str.Trim();

        return str;
    }
}
