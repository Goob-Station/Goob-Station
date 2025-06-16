using Content.Server.Administration;
using Content.Server.Administration.Components;
using Content.Server.Chat.Managers;
using Content.Server.Power.Components;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;

namespace Content.Server._Pirate.Administration.Commands
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
            var query = entityManager.EntityQueryEnumerator<StationInfiniteBatteryTargetComponent, BatteryComponent, MetaDataComponent>();
            while (query.MoveNext(out var uid, out _, out var battery, out var metadata))
            {
                if (metadata.EntityPrototype is not { ID: "SMESBasic", })
                    continue;

                var recharger = entityManager.EnsureComponent<BatterySelfRechargerComponent>(uid);
                recharger.AutoRecharge = true;
                recharger.AutoRechargeRate = battery.MaxCharge; // Instant refill.
                recharger.AutoRechargePause = false; // No delay.
            }

            shell.WriteLine("Виконано!");
        }
    }
}
