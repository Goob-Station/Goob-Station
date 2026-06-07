using Content.Shared._White.Xenomorphs.Larva;
using Content.Shared.Mobs;
using Robust.Shared.Player;

namespace Content.Server.Mobs;

public sealed partial class CritMobActionsSystem
{
    /// <summary>
    /// Prevents the host of the Xenomorph Larva from killing themselves,
    /// to allow the larva to develop.
    /// </summary>
    private void PreventLarvaHostDeath(EntityUid uid, ActorComponent actor, CritSuccumbEvent args)
    {
        if (HasComp<XenomorphLarvaVictimComponent>(uid))
        {
            // We are using suicide here because it's already coded to ghost the player,
            // while allowing them to return to their body.
            _host.ExecuteCommand(actor.PlayerSession, "suicide");

            args.Handled = true;
            return;
        }
    }
}
