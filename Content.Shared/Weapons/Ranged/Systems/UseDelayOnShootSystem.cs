// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Timing;
using Content.Shared.Weapons.Ranged.Components;

namespace Content.Shared.Weapons.Ranged.Systems;

public sealed class UseDelayOnShootSystem : EntitySystem
{
    [Dependency] private readonly UseDelaySystem _delay = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<UseDelayOnShootComponent, GunShotEvent>(OnUseShoot);
    }

    private void OnUseShoot(Entity<UseDelayOnShootComponent> ent, ref GunShotEvent args)
    {
        if (TryComp(ent, out UseDelayComponent? useDelay))
            _delay.TryResetDelay((ent, useDelay));
    }
}