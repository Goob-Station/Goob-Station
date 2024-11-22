using Content.Shared.Antag;
using Content.Shared.StatusIcon;
using Robust.Shared.Prototypes;

namespace Content.Shared.WhiteDream.BloodCult.Constructs;

[RegisterComponent]
public sealed partial class ConstructComponent : Component, IAntagStatusIconComponent
{
    [DataField]
    public List<EntProtoId> Actions = new();

    /// <summary>
    ///     Used by the client to determine how long the transform animation should be played.
    /// </summary>
    [DataField]
    public float TransformDelay = 1;

    [DataField]
    public ProtoId<StatusIconPrototype> StatusIcon { get; set; } = "BloodCultMember";

    [DataField]
    public bool IconVisibleToGhost { get; set; } = true;

    public bool Transforming = false;
    public float TransformAccumulator = 0;

    public List<EntityUid?> ActionEntities = new();
}
