using System;
using Content.Server.Administration;
using Content.Server.Administration.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Administration.Components;
using Content.Server.Power.Components;
using Content.Shared.Administration;
using Content.Shared.Power.Components;
using Robust.Shared.Console;

namespace Content.Pirate.Server.Administration.Commands
{
    [AdminCommand(AdminFlags.Admin)]
    public sealed class SmesUnlimCommand : IConsoleCommand
    {
        public string Command => "smes-unlim";
        public string Description => "Усі смеси в грі перезаряджаються як дурні по кругу";
        public string Help => $"{Command} для безлім смесів всіх що існують зараз у грі";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var entityManager = IoCManager.Resolve<IEntityManager>();
            var batterySystem = entityManager.System<BatterySystem>();
            var query = entityManager
                .EntityQueryEnumerator<StationInfiniteBatteryTargetComponent, BatteryComponent, MetaDataComponent>();
            while (query.MoveNext(out var uid, out _, out var battery, out var metadata))
            {
                if (metadata.EntityPrototype is not { ID: "SMESBasic", })
                    continue;

                var recharger = entityManager.EnsureComponent<BatterySelfRechargerComponent>(uid);
                recharger.AutoRechargeRate = battery.MaxCharge; // Instant refill.
                recharger.AutoRechargePauseTime = TimeSpan.Zero; // No delay.
                recharger.NextAutoRecharge = null; // Recharge immediately.
                entityManager.Dirty(uid, recharger);
                batterySystem.RefreshChargeRate((uid, battery));
            }

            shell.WriteLine("Виконано!");
        }
    }
}
