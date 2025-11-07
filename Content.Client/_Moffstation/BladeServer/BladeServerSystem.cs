using Content.Client.Examine;
using Content.Client.Verbs.UI;
using Content.Shared._Moffstation.BladeServer;
using Robust.Client.UserInterface;

namespace Content.Client._Moffstation.BladeServer;

/// <summary>
/// This system extends <see cref="SharedBladeServerSystem"/> with accessors for BUI interactions.
/// </summary>
public sealed partial class BladeServerSystem : SharedBladeServerSystem
{
    [Dependency] private readonly IUserInterfaceManager _userInterfaceMan = default!;
    [Dependency] private readonly ExamineSystem _examine = default!;

    /// <summary>
    /// Relays examine interactions to <see cref="_userInterfaceMan"/> for the entity in the given
    /// <paramref name="slotIndex"/>. Returns true if the interaction was handled, false otherwise.
    /// </summary>
    public bool TryExamine(Entity<BladeServerRackComponent?> entity, int slotIndex)
    {
        if (GetSlotOrNull(entity, slotIndex)?.Item is not { } bladeServer)
            return false;

        _examine.DoExamine(bladeServer);
        return true;
    }

    /// <summary>
    /// Relays context-menu interactions to <see cref="_userInterfaceMan"/> for the entity in the given
    /// <paramref name="slotIndex"/>. Returns true if the interaction was handled, false otherwise.
    /// </summary>
    public bool TryUseSecondary(Entity<BladeServerRackComponent?> entity, int slotIndex)
    {
        if (GetSlotOrNull(entity, slotIndex)?.Item is not { } bladeServer)
            return false;

        _userInterfaceMan.GetUIController<VerbMenuUIController>().OpenVerbMenu(bladeServer);
        return true;
    }
}
