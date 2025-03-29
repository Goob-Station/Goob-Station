using Content.Shared.Actions;
using Content.Shared.Buckle.Components;
using Content.Shared.Climbing.Components;
using Content.Shared.CombatMode;
using Content.Shared.DoAfter;
using Content.Shared.Emag.Systems;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Item;
using Content.Shared.Physics;
using Content.Shared.Stunnable;
using Content.Shared.Vehicles;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Whitelist;
using Robust.Shared.Containers;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;
using Robust.Shared.Audio.Systems;
using Content.Shared.DragDrop;
using Content.Shared.Emag.Components;

namespace Content.Shared._Goobstation.Vehicles.Clowncar;

public abstract partial class SharedClowncarSystem
{
    /// <summary>
    /// Handles activating/deactivating the cannon when requested
    /// </summary>
    private void OnClowncarFireModeAction(EntityUid uid, ClowncarComponent component, ClowncarFireModeActionEvent args)
    {
        if (args.Handled)
            return;

        ToggleCannon(uid, component, args.Performer, true);//component.CannonEntity == null);
        args.Handled = true;
    }
}
