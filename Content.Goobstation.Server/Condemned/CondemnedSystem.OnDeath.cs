// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.CheatDeath;
using Content.Shared.Mobs;

namespace Content.Goobstation.Server.Condemned;
public sealed class CondemnedOnDeathSystem : EntitySystem
{
    [Dependency] private readonly CondemnedSystem _condemned = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CondemnedComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnMobStateChanged(EntityUid uid, CondemnedComponent comp, MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead || comp.SoulOwnedNotDevil || !comp.CondemnOnDeath)
            return;

        if (TryComp<CheatDeathComponent>(uid, out var cheatDeath) && cheatDeath.ReviveAmount > 0)
            return;

        _condemned.StartCondemnation(uid, comp, behavior: CondemnedSystem.CondemnedBehavior.Delete);
    }
}
