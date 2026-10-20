// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using Content.Goobstation.Shared.Silicon.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Power.Components;
using Content.Shared.Power.EntitySystems;
using Content.Shared.PowerCell;
using Content.Shared.Silicons.Borgs.Components;
using Robust.Shared.Containers;

namespace Content.Goobstation.Server.Silicon;

public sealed partial class BorgBatteryDrainRechargeSystem : EntitySystem
{
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private PowerCellSystem _powerCell = default!;
    [Dependency] private SharedBatterySystem _battery = default!;
    [Dependency] private MobStateSystem _mobState = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<BorgBatteryDrainRechargeComponent, BatteryComponent>();
        while (query.MoveNext(out var uid, out var recharger, out var battery))
        {
            if (_battery.IsFull((uid, battery)))
                continue;

            if (!TryGetBorgChassis(uid, out var chassis))
                continue;

            if (_mobState.IsDead(chassis.Value))
                continue;

            if (!_powerCell.TryGetBatteryFromSlot(chassis.Value, out var chassisBattery))
                continue;

            var currentCharge = _battery.GetCharge((uid, battery));
            var missingCharge = battery.MaxCharge - currentCharge;
            var chargeToAdd = MathF.Min(recharger.RechargeRate * frameTime, missingCharge);
            if (chargeToAdd <= 0f)
                continue;

            var drainAmount = chargeToAdd * recharger.DrainMultiplier;
            if (!_battery.TryUseCharge((chassisBattery.Value.Owner, chassisBattery.Value.Comp), drainAmount))
                continue;

            _battery.ChangeCharge((uid, battery), chargeToAdd);
        }
    }

    private bool TryGetBorgChassis(EntityUid item, [NotNullWhen(true)] out EntityUid? chassis)
    {
        var current = item;
        for (var i = 0; i < 10; i++)
        {
            if (!_container.TryGetContainingContainer(current, out var container))
                break;

            current = container.Owner;
            if (HasComp<BorgChassisComponent>(current))
            {
                chassis = current;
                return true;
            }
        }

        chassis = null;
        return false;
    }
}
