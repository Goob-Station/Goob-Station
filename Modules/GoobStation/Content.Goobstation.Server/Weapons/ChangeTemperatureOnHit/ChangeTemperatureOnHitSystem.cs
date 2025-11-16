// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Temperature.Systems;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Goobstation.Server.Weapons.ChangeTemperatureOnHit;

public sealed class ChangeTemperatureOnHitSystem : EntitySystem
{
    [Dependency] private readonly TemperatureSystem _temperature = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangeTemperatureOnHitComponent, MeleeHitEvent>(OnHit);
    }

    private void OnHit(Entity<ChangeTemperatureOnHitComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit)
            return;

        var comp = ent.Comp;

        foreach (var target in args.HitEntities)
        {
            _temperature.ChangeHeat(target, comp.Heat, comp.IgnoreResistances);
        }
    }
}