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
