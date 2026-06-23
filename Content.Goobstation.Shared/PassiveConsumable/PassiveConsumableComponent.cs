using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Inventory;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.PassiveConsumable;

/// <summary>
/// When equipped in the specified clothing slot, passively consumes the item's solution
/// and transfers it to the wearer's stomach at a fixed interval.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
[Access(typeof(PassiveConsumableSystem))]
public sealed partial class PassiveConsumableComponent : Component
{
    /// <summary>
    /// Amount of solution transferred per consumption.
    /// </summary>
    [DataField, AutoNetworkedField]
    public FixedPoint2 Amount = FixedPoint2.New(0.1);

    /// <summary>
    /// The entity currently wearing this item. Null if not worn.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Wearer;

    /// <summary>
    /// Which clothing slot must this be equipped in for passive consumption.
    /// </summary>
    [DataField, AutoNetworkedField]
    public SlotFlags Slot = SlotFlags.MASK;

    /// <summary>
    /// When the next consumption will occur.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan NextConsume;

    /// <summary>
    /// Time between consumptions.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan ConsumeInterval = TimeSpan.FromSeconds(3);

    /// <summary>
    /// Whether to delete the entity (spawning trash from <see cref="Content.Shared.Nutrition.Components.EdibleComponent"/>)
    /// when the solution is fully consumed.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool DeleteOnEmpty = true;
}
