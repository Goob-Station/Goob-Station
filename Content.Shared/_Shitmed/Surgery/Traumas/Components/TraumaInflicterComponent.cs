using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;

[RegisterComponent, AutoGenerateComponentState, NetworkedComponent]
public sealed partial class TraumaInflicterComponent : Component
{
    /// <summary>
    /// I really don't like severity check hardcode; So, I will be putting this here, if the severity of the wound is lesser than this, the trauma won't be induced
    /// </summary>
    [DataField, AutoNetworkedField]
    public FixedPoint2 SeverityThreshold = 9f;

    /// <summary>
    /// The container where all the traumas are stored
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public Container TraumaContainer = new();

    /// <summary>
    /// If present in the list, when trauma of the said type is applied, the armour will be counted in to the deduction
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public List<ProtoId<TraumaTypePrototype>> AllowArmourDeduction = new();

    /// <summary>
    /// Optional per-inflicter override of the entity spawned for a trauma type.
    /// When a type is not present here, <see cref="TraumaTypePrototype.TraumaEntity"/> is used.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public Dictionary<ProtoId<TraumaTypePrototype>, EntProtoId> TraumaPrototypes = new();

    /// <summary>
    /// Additional chance (-1, 0, 1) that is added in chance calculation
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<ProtoId<TraumaTypePrototype>, FixedPoint2> TraumasChances = new();

    /// <summary>
    /// When a wound is mangled, any receiving damage will be multiplied by these values and applied to the respective body elements.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<ProtoId<TraumaTypePrototype>, FixedPoint2>? MangledMultipliers;
}
