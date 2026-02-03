using System.Text;
using Content.Shared.Inventory;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;


namespace Content.Pirate.Shared.Clothing.SlotBlocker;


/// <summary>
///     Applied to clothing that can block and be blocked by other clothing.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SlotBlockerComponent : Component
{
    [DataField, AutoNetworkedField, AlwaysPushInheritance]
    public BlockerDefinition Blocks, BlockedBy;

    [DataField]
    public bool IgnoreOtherBlockers = false;
}

[DataDefinition, Serializable, NetSerializable]
public partial struct BlockerDefinition
{
    /// <summary>
    ///     Slots that block this clothing or get blocked by this clothing.
    /// </summary>
    [DataField]
    public SlotFlags Slots = SlotFlags.NONE;

    [DataField]
    public EntityWhitelist? Whitelist, Blacklist;

    [DataField]
    public bool PreventsEquip = true, PreventsUnequip = true;

    /// <summary>
    ///     Will only prevent equipping/unequipping ONLY if the BLOCKER is in one of these slots (not the equipment).
    ///     Excludes pockets by default.
    /// </summary>
    [DataField]
    public SlotFlags EnableInSlots = SlotFlags.WITHOUT_POCKET;

    #region utility
    public override string ToString() => $"Blocker(Slots: {SlotsToString()}, E/U: {PreventsEquip}/{PreventsUnequip}, Whitelist: {Whitelist}, Blacklist: {Blacklist})";

    public string SlotsToString()
    {
        var values = Enum.GetValues<SlotFlags>();
        var sb = new StringBuilder();

        foreach (var slot in values)
        {
            if (!Slots.HasFlag(slot) || !Loc.TryGetString("slot-name-" + Enum.GetName(slot), out var locName))
                continue;

            if (sb.Length > 0)
                sb.Append(", ");

            sb.Append(locName);
        }

        return sb.ToString();
    }
    #endregion
}
