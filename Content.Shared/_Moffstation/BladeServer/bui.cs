using Robust.Shared.Serialization;

namespace Content.Shared._Moffstation.BladeServer;

/// <summary>
/// This enum is used to identify the <see cref="BladeServerRackComponent"/>'s associated UI.
/// </summary>
[Serializable, NetSerializable]
public enum BladeServerRackUiKey : byte
{
    Key,
}

/// <summary>
/// This class contains the state of a <see cref="BladeServerRackComponent"/>'s associated BUI.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class BladeServerRackBoundUserInterfaceState(
    List<BladeServerRackBoundUserInterfaceState.Slot> slots
) : BoundUserInterfaceState
{
    public readonly List<Slot> Slots = slots;

    [Serializable, NetSerializable]
    public record Slot(NetEntity? Entity, bool Powered, bool Locked);
}
