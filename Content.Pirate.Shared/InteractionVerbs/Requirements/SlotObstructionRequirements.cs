using Content.Pirate.Shared.Clothing.SlotBlocker;
using Content.Shared.Clothing.Components;
using Content.Shared.InteractionVerbs;
using Content.Shared.Inventory;
using Content.Shared.Whitelist;
using Robust.Shared.Serialization;

namespace Content.Pirate.Shared.InteractionVerbs.Requirements;


/// <summary>
///     Requires the target to have the slot obstructed (or not obstructed) by anything (e.g. a mask) that passes the whitelist
///     and (optionally) not have it blocked.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class SlotObstructionRequirement : InteractionRequirement
{
    /// <summary>
    ///     If true, the user is checked. Otherwise, the target.
    /// </summary>
    [DataField] public bool CheckUser = true;

    [DataField] public SlotFlags Slot;

    /// <summary>
    ///     When to fail this condition. If the entity has no matching slot, it is assumed to be non-obstructed.
    /// </summary>
    [DataField]
    public bool
        MustBeObstructed = false,
        MustNotBeObstructed = true;

    [DataField] public EntityWhitelist? ObstructorWhitelist;

    /// <summary>
    ///     Whether to ignore e.g. a mask that has been pulled down.
    /// </summary>
    [DataField] public bool IgnoreIfPulledDown = true;

    /// <summary>
    ///     Whether the slot must/must not be blocked. See <see cref="SlotBlockerSystem"/> for more details.
    /// </summary>
    [DataField]
    public bool
        MustBeBlocked = false,
        MustNotBeBlocked = true;


    public override bool IsMet(InteractionArgs args, InteractionVerbPrototype proto, InteractionAction.VerbDependencies deps)
    {
        var target = CheckUser ? args.User : args.Target;
        if (!deps.EntMan.TryGetComponent(target, out InventoryComponent? inventory))
            return MustNotBeObstructed;

        var checkBlockers = MustBeBlocked || MustNotBeBlocked;
        var checkObstruction = MustBeObstructed || MustNotBeObstructed;

        if (checkObstruction)
        {
            var slots = inventory.Slots;
            var containers = inventory.Containers;

            for (var i = 0; i < slots.Length; i++)
            {
                if ((slots[i].SlotFlags & Slot) == 0)
                    continue;

                var obstructed = false;
                if (containers[i].ContainedEntity is { } clothing)
                {
                    if (IgnoreIfPulledDown &&
                        deps.EntMan.TryGetComponent<MaskComponent>(clothing, out var mask) && mask.IsToggled)
                        continue;

                    obstructed = ObstructorWhitelist == null || deps.WhitelistSystem.IsWhitelistPass(ObstructorWhitelist, clothing);
                }

                // A matching slot is found. We assume there can only be one.
                if (MustBeObstructed && !obstructed || MustNotBeObstructed && obstructed)
                    return false;
            }
        }

        if (checkBlockers)
        {
            var blockerSys = deps.EntMan.System<SlotBlockerSystem>();
            var blocked = blockerSys.IsSlotObstructed(
                (target, inventory),
                null,
                SlotBlockerSystem.CheckType.IgnoreBlockerPreference,
                Slot,
                out _);

            if (MustBeBlocked && !blocked || MustNotBeBlocked && blocked)
                return false;
        }

        return true;
    }
}
