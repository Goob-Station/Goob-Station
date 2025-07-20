// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Heretic.Components;

namespace Content.Server.Heretic.EntitySystems.PathSpecific;

public sealed class SilverMaelstromSystem : EntitySystem
{
    [Dependency] private readonly ProtectiveBladeSystem _pblade = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SilverMaelstromComponent, ProtectiveBladeUsedEvent>(OnBladeUsed);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var eqe = EntityQueryEnumerator<SilverMaelstromComponent>();
        while (eqe.MoveNext(out var uid, out var smc))
        {
            if (!uid.IsValid())
                continue;

            smc.RespawnTimer -= frameTime;

            if (smc.RespawnTimer <= 0)
            {
                smc.RespawnTimer = smc.RespawnCooldown;

                if (smc.ActiveBlades < smc.MaxBlades)
                {
                    _pblade.AddProtectiveBlade(uid);
                    smc.ActiveBlades += 1;
                }
            }
        }
    }

    private void OnBladeUsed(Entity<SilverMaelstromComponent> ent, ref ProtectiveBladeUsedEvent args)
    {
        // using max since ascended heretic can spawn more blades with furious steel action
        ent.Comp.ActiveBlades = Math.Max(ent.Comp.ActiveBlades - 1, 0);
    }
}
