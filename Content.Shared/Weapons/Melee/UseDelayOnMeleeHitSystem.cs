// SPDX-FileCopyrightText: 2025 ActiveMammmoth <140334666+ActiveMammmoth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 ActiveMammmoth <kmcsmooth@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 keronshb <54602815+keronshb@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Throwing;
using Content.Shared.Timing;
using Content.Shared.Weapons.Melee.Components;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Shared.Weapons.Melee;

/// <inheritdoc cref="UseDelayOnMeleeHitComponent"/>
public sealed class UseDelayOnMeleeHitSystem : EntitySystem
{
    [Dependency] private readonly UseDelaySystem _delay = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<UseDelayOnMeleeHitComponent, MeleeHitEvent>(OnMeleeHit);
        SubscribeLocalEvent<UseDelayOnMeleeHitComponent, ThrowDoHitEvent>(OnThrowHitEvent);
    }

    private void OnThrowHitEvent(Entity<UseDelayOnMeleeHitComponent> ent, ref ThrowDoHitEvent args)
    {
        TryResetDelay(ent);
    }

    private void OnMeleeHit(Entity<UseDelayOnMeleeHitComponent> ent, ref MeleeHitEvent args)
    {
        TryResetDelay(ent);
    }

    private void TryResetDelay(Entity<UseDelayOnMeleeHitComponent> ent)
    {
        var uid = ent.Owner;

        if (!TryComp<UseDelayComponent>(uid, out var useDelay))
            return;

        _delay.TryResetDelay((uid, useDelay), checkDelayed: true);
    }
}