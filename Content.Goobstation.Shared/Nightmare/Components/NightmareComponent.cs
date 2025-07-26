using Content.Shared.Polymorph;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Nightmare.Components;

/// <summary>
/// This is used for nightmares
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class NightmareComponent : Component
{
    [DataField]
    public EntProtoId ActionPlaneShift = "ActionPlaneShift";

    [ViewVariables]
    public EntityUid? ActionPlaneShiftEntity;

    [DataField]
    public EntProtoId ActionLightEater = "ActionLightEater";

    [ViewVariables]
    public EntityUid? ActionLightEntity;

    [DataField]
    public ProtoId<PolymorphPrototype> ShadowSpeciesProto = "ShadowNightmarePolymorph";
}
