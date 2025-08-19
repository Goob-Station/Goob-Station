// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Flash.Components;
using Content.Shared.Damage;

namespace Content.Server.Flash;
public sealed class DamagedByFlashingSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DamagedByFlashingComponent, FlashAttemptEvent>(OnFlashAttempt);
    }
    private void OnFlashAttempt(Entity<DamagedByFlashingComponent> ent, ref FlashAttemptEvent args)
    {
        _damageable.TryChangeDamage(ent, ent.Comp.FlashDamage);

        //TODO: It would be more logical if different flashes had different power,
        //and the damage would be inflicted depending on the strength of the flash.
    }
}