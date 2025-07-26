// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Heretic.Components;
using Content.Goobstation.Shared.Heretic.Systems;
using Robust.Client.GameObjects;

namespace Content.Client._Shitcode.Heretic;

public sealed class ShadowCloakSystem : SharedShadowCloakSystem
{
    protected override void Startup(Entity<ShadowCloakedComponent> ent)
    {
        base.Startup(ent);

        if (!TryComp(ent, out SpriteComponent? sprite))
            return;

        ent.Comp.WasVisible = sprite.Visible;
        sprite.Visible = false; // todo: SLOP!!!
    }

    protected override void Shutdown(Entity<ShadowCloakedComponent> ent)
    {
        base.Shutdown(ent);

        if (TryComp(ent, out SpriteComponent? sprite))
            sprite.Visible = ent.Comp.WasVisible;
    }
}
