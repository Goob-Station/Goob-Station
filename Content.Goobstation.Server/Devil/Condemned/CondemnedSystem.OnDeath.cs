// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.CheatDeath;
using Content.Shared.Mobs;

namespace Content.Goobstation.Server.Devil.Condemned;
public sealed partial class CondemnedSystem
{
    public void InitializeOnDeath()
    {
        SubscribeLocalEvent<CondemnedComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnMobStateChanged(EntityUid uid, CondemnedComponent comp, MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead || comp.SoulOwnedNotDevil || !comp.CondemnOnDeath)
            return;

        if (TryComp<CheatDeathComponent>(uid, out var cheatDeath) && cheatDeath.ReviveAmount > 0)
            return;

        StartCondemnation(uid, comp, behavior: CondemnedBehavior.Delete);
    }
}
