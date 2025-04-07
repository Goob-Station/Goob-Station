// SPDX-FileCopyrightText: 2025 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared._RMC14.GhostColor;
using Robust.Client.GameObjects;

namespace Content.Client._RMC14.GhostColor;

public sealed class GhostColorSystem : EntitySystem
{
    public override void Update(float frameTime)
    {
        var defaultColor = Color.FromHex("#FFFFFF88");
        var colors = EntityQueryEnumerator<GhostColorComponent, SpriteComponent>();
        while (colors.MoveNext(out var color, out var sprite))
        {
            sprite.Color = color.Color ?? defaultColor;
        }
    }
}
