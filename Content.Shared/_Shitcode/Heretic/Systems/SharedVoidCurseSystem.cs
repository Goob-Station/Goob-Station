// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared.Heretic;
using Content.Shared.Mobs.Components;

namespace Content.Shared._Goobstation.Heretic.Systems;

public abstract partial class SharedVoidCurseSystem : EntitySystem
{
    protected virtual void Cycle(Entity<VoidCurseComponent> ent)
    {

    }

    public void DoCurse(EntityUid uid)
    {
        if (!HasComp<MobStateComponent>(uid))
            return; // ignore non mobs because holy shit

        if (TryComp<HereticComponent>(uid, out var h) && h.CurrentPath == "Void" || HasComp<GhoulComponent>(uid))
            return;

        if (TryComp<VoidCurseComponent>(uid, out var curse))
        {
            if (!curse.Drain)
            {
                // we keep adding curse time until we reach ~30 seconds
                // when the time is reached it can't add any more time to the curse and just locks itself out until it's gone
                // which is very balanced :+1:
                curse.Lifetime = Math.Clamp(curse.Lifetime + 5f, 0f, curse.MaxLifetime);
                if (curse.Lifetime >= curse.MaxLifetime)
                    curse.Drain = true;
            }
            curse.Stacks = Math.Clamp(curse.Stacks + 1, 0, curse.MaxStacks + 1);
            Dirty(uid, curse);
        }
        else EnsureComp<VoidCurseComponent>(uid);
    }
}