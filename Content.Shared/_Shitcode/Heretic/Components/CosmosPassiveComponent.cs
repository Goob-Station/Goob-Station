using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CosmosPassiveComponent : Component
{
    public override bool SessionSpecific => true;

    [DataField]
    public float StaminaHeal = -15f;

    // Storing ref to star gazer here cause why not
    [DataField, AutoNetworkedField]
    public EntityUid? StarGazer;

    [DataField]
    public EntProtoId StarGazerId = "MobStarGazer";
}
