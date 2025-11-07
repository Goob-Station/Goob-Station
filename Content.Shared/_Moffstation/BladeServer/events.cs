using Robust.Shared.Serialization;

namespace Content.Shared._Moffstation.BladeServer;

/// <summary>
/// The base type of messages raised on entities with <see cref="BladeServerRackComponent"/> from its associated UI.
/// </summary>
[Serializable, NetSerializable]
public abstract class BaseBladeServerRackMessage(int index) : BoundUserInterfaceMessage
{
    /// <summary>
    /// The index of the blade slot in the rack which was interacted with.
    /// </summary>
    public readonly int Index = index;
}

/// <summary>
/// Raised when a user clicks on the "Eject" button for a blade slot.
/// </summary>
/// <see cref="BaseBladeServerRackMessage"/>
[Serializable, NetSerializable]
public sealed partial class BladeServerRackEjectPressedMessage(int index) : BaseBladeServerRackMessage(index);

/// <summary>
/// Raised when a user clicks on the "Insert" button for a blade slot.
/// </summary>
/// <see cref="BaseBladeServerRackMessage"/>
[Serializable, NetSerializable]
public sealed partial class BladeServerRackInsertPressedMessage(int index) : BaseBladeServerRackMessage(index);

/// <summary>
/// Raised when a user clicks on the "Power" button for a blade slot.
/// </summary>
/// <see cref="BaseBladeServerRackMessage"/>
[Serializable, NetSerializable]
public sealed partial class BladeServerRackPowerPressedMessage(int index, bool powered) : BaseBladeServerRackMessage(index)
{
    /// <summary>
    /// The state of powered-ness to switch to.
    /// </summary>
    public readonly bool Powered = powered;
}

/// <summary>
/// Raised when a user clicks on the entity representation button for a blade slot. This is used to relay interactions
/// from the UI "into" the rack to affect a blade server.
/// </summary>
/// <see cref="BaseBladeServerRackMessage"/>
[Serializable, NetSerializable]
public sealed partial class BladeServerRackUseMessage(int index) : BaseBladeServerRackMessage(index);

/// <summary>
/// Raised when a user clicks on the entity representation button for a blade slot. This is used to relay interactions
/// from the UI "into" the rack to affect a blade server.
/// </summary>
/// <see cref="BaseBladeServerRackMessage"/>
[Serializable, NetSerializable]
public sealed partial class BladeServerRackActivateInWorldMessage(int index, bool alternate) : BaseBladeServerRackMessage(index)
{
    public readonly bool Alternate = alternate;
}
