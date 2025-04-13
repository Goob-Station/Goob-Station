// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Medical;
using Content.Goobstation.Shared.Medical.Components;
using Robust.Client.GameObjects;

namespace Content.Goobstation.Client.Medical;

public sealed class MedigunSystem : SharedMedigunSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MediGunHealedComponent, ComponentStartup>(OnMedigunHealed);
        SubscribeLocalEvent<MediGunHealedComponent, ComponentShutdown>(OnMedigunShutdown);
    }

    private void OnMedigunHealed(Entity<MediGunHealedComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
        {
            return;
        }

        if (TryComp<MediGunComponent>(ent.Comp.Source, out var medigun))
        {
            sprite.Color = medigun.UberActivated ? medigun.UberLineColor : medigun.DefaultLineColor;
        }
    }

    private void OnMedigunShutdown(Entity<MediGunHealedComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
        {
            return;
        }

        sprite.Color = Color.White;
    }
}
