using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Factory;

[RegisterComponent, NetworkedComponent, Access(typeof(RoboticArmSystem))]
[AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class RoboticArmComponent : Component
{
    /// <summary>
    /// Item slot that stores the held item.
    /// </summary>
    [DataField]
    public string ItemSlotId = "robotic_arm_item";

    /// <summary>
    /// The item slot cached on init.
    /// </summary>
    [ViewVariables]
    public ItemSlot ItemSlot = default!;

    /// <summary>
    /// The currently held item.
    /// </summary>
    public EntityUid? HeldItem => ItemSlot.Item;

    /// <summary>
    /// Whether an item is currently held.
    /// </summary>
    public bool HasItem => ItemSlot.HasItem;

    /// <summary>
    /// When the arm will next move to the input or output.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoNetworkedField, AutoPausedField]
    public TimeSpan? NextMove;

    /// <summary>
    /// How long it takes to move an item.
    /// </summary>
    [DataField]
    public TimeSpan MoveDelay = TimeSpan.FromSeconds(0.6);

    /// <summary>
    /// Fixture to look for input items with when no input machine is linked.
    /// </summary>
    [DataField]
    public string InputFixtureId = "robotic_arm_input";

    /// <summary>
    /// Items currently colliding with <see cref="InputFixtureId"/> and whether their CollisionWake was enabled.
    /// When items start to collide they get pushed to the end.
    /// When picking up items the last value is taken.
    /// This is essentially a FILO queue.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<(NetEntity, bool)> InputItems = new();

    /// <summary>
    /// Sound played when moving an item.
    /// </summary>
    [DataField]
    public SoundSpecifier? MoveSound;
}

[Serializable, NetSerializable]
public enum RoboticArmVisuals : byte
{
    HasItem
}

[Serializable, NetSerializable]
public enum RoboticArmLayers : byte
{
    Arm,
    Powered
}
