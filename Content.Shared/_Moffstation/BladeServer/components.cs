using System.Numerics;
using Content.Shared.Construction.Components;
using Content.Shared.Containers.ItemSlots;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Moffstation.BladeServer;

/// This component makes an entity into a Blade Server Rack, which is a container for
/// <see cref="BladeServerComponent">blade servers</see>.
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(raiseAfterAutoHandleState: true),
 Access(typeof(SharedBladeServerSystem))]
public sealed partial class BladeServerRackComponent : Component
{
    /// The number of slots for blade servers in this rack. Note that this is NOT the number of servers in the rack, but
    /// rather the available capacity for them.
    [DataField, AutoNetworkedField]
    public int NumSlots = 4;

    /// The entities to spawn into this rack when this component is initialized. The order in the list corresponds to
    /// the slots, so a <c>[null, "some ent"]</c> StartingContents would leave the first slot empty and spawn an entity
    /// for the second slot.
    /// Note that this field is used ONLY on initialization and does nothing during runtime.
    [DataField(readOnly: true), ViewVariables(VVAccess.ReadOnly)]
    public List<EntProtoId?> StartingContents = [];

    /// The actual contents of this rack as represented by <see cref="BladeSlot"/>s.
    [ViewVariables(VVAccess.ReadOnly)]
    public readonly List<BladeSlot> BladeSlots = [];

    /// A prefix used to create unique <see cref="BladeSlotName"/>s. It is used for decontainer IDs mostly and is
    /// specifiable in YAML so YAMLers can do complex things with containers.
    [DataField(readOnly: true), ViewVariables(VVAccess.ReadOnly)]
    public string BladeSlotNamePrefix = "blade-server-rack-slot";

    /// The visual offset, in pixels, of one blade slot to another in the rack's sprite. So if the second blade should
    /// be four pixels below the first, this should be <c>[0, -4]</c>.
    [DataField]
    public Vector2i BladeServerVisualsOffset;

    /// Generates a name for the <paramref name="index"/>'th slot in this rack.
    /// <param name="index"></param>
    /// <returns></returns>
    public string BladeSlotName(int index) => $"{BladeSlotNamePrefix}-{index}";

    /// Calculates the sprite layer offset for the <paramref name="index"/>'th slot in this rack.
    // TODO 32f here is a gross magic number corresponding to pixels per tile. I regret it, but I couldn't find a constant to replace it.
    public Vector2 BladeSlotSpritePixelOffsetToLayerOffset(int index) => BladeServerVisualsOffset * index / 32f;
}

/// This data class holds onto information about a single slot in a <see cref="BladeServerRackComponent"/>.
public sealed partial class BladeSlot(ItemSlot slot)
{
    /// The <see cref="ItemSlot"/> corresponding to this blade slot.
    [ViewVariables(VVAccess.ReadOnly)]
    public readonly ItemSlot Slot = slot;

    /// Whether or not this blade slot's power toggle is on. Note that this DOES NOT make a blade powered or not, as the
    /// overall rack has a separate power state which also contributes to blades being powered.
    [ViewVariables(VVAccess.ReadOnly)]
    public bool IsPowerEnabled = true;

    /// The entity in this slot.
    public EntityUid? Item => Slot.Item;

    /// Whether or not this slot is currently ejecting. This is something of a hack to enable a UI button to eject
    /// the contents of this slot while disallowing other mechanisms which would remove the contents of this slot
    /// (eg. picking up by hand).
    [Access(typeof(SharedBladeServerSystem))]
    public bool Ejecting = false;
}

/// This component makes an entity a Blade Server capable of being inserted into a <see cref="BladeServerRackComponent"/>.
[RegisterComponent, AutoGenerateComponentState]
public sealed partial class BladeServerComponent : Component
{
    /// The color of the stripe on this blade server, if any. This just makes different blade servers visually distinct.
    [DataField, AutoNetworkedField]
    public Color? StripeColor;
}

/// This marker component is just used to distinguish when a <see cref="Content.Server.Construction.Components.MachineFrameComponent"/> is
/// Blade Server Frame and thus should be able to accept <see cref="BladeServerBoardComponent">blade server boards</see>.
[RegisterComponent, NetworkedComponent]
public sealed partial class BladeServerFrameComponent : Component;

/// This component is a marker similar to <see cref="BladeServerFrameComponent"/> for distinguishing from
/// <see cref="MachineBoardComponent"/>, but it is also used to store information about how construction of the blade
/// server works.
[RegisterComponent, NetworkedComponent]
public sealed partial class BladeServerBoardComponent : Component
{
    [DataField(required: true)]
    public EntProtoId Prototype;
}
