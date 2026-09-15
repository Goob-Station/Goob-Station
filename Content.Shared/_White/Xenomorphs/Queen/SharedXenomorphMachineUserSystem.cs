using Content.Shared.Interaction;
using Content.Shared.UserInterface;

namespace Content.Shared._White.Xenomorphs.Queen;

/// <summary>
/// Lets xenomorph queens/empresses use consoles and machines even while in combat mode.
/// Pickup of weapons/items remains blocked by <see cref="Xenomorph.SharedXenomorphSystem"/>.
/// </summary>
public sealed class SharedXenomorphMachineUserSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<XenomorphMachineUserComponent, CombatModeCanInteractNonItemEvent>(OnCombatNonItem);
    }

    private void OnCombatNonItem(EntityUid uid, XenomorphMachineUserComponent component, ref CombatModeCanInteractNonItemEvent args)
    {
        // Only ActivatableUI (consoles/machines). Broad UserInterface matches (e.g. goliath surgery)
        // would treat combat clicks as hand-interact and block normal melee.
        if (HasComp<ActivatableUIComponent>(args.Target))
            args.CanInteract = true;
    }
}

