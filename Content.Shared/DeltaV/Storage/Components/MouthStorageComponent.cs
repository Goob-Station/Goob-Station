using Content.Shared.DeltaV.Storage.EntitySystems;
using Content.Shared.FixedPoint;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
namespace Content.Shared.DeltaV.Storage.Components;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
[Access(typeof(SharedMouthStorageSystem))]
public sealed partial class MouthStorageComponent : Component
{
    public const string MouthContainerId = "mouth";

    [DataField("openStorageAction"), AutoNetworkedField]
    public EntProtoId? OpenStorageAction;

    [DataField, AutoNetworkedField]
    public EntityUid? Action;

    [DataField("mouthProto")]
    public EntProtoId MouthProto = "ActionOpenMouthStorage";

    [ViewVariables]
    public Container Mouth = default!;

    [ViewVariables]
    [DataField]
    public EntityUid? MouthId;

    // Mimimum inflicted damage on hit to spit out items
    [DataField("spitDamageThreshold")]
    public FixedPoint2 SpitDamageThreshold = FixedPoint2.New(2);
}
