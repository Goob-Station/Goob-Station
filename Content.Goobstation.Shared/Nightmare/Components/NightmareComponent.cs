using Content.Shared.Polymorph;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;


namespace Content.Shared._EE.Nightmare.Components;


/// <summary>
/// This is used for nightmares
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class NightmareComponent : Component
{
    public string ActionPlaneShift = "ActionPlaneShift";
    public EntityUid? ActionPlaneShiftEntity;

    public string ActionLightEater = "ActionLightEater";
    public EntityUid? ActionLightEntity;

    [DataField]
    public ProtoId<PolymorphPrototype> ShadowSpeciesProto = "ShadowNightmarePolymorph";
}
