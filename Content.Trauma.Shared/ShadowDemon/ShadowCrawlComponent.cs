using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Trauma.Shared.ShadowDemon;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class ShadowCrawlComponent : Component
{
    [DataField]
    public EntProtoId PhaseIn;

    [DataField]
    public EntProtoId PhaseOut;

    [DataField]
    public float DamageModiferFromLights = 0.5f;

    [DataField]
    public float SpeedBuff = 2;

    [DataField]
    public TimeSpan ActionCooldown = TimeSpan.FromSeconds(4);

    [ViewVariables, AutoNetworkedField]
    public bool Active;

    [DataField]
    public EntProtoId Action;

    [ViewVariables]
    public EntityUid? ActionUid;
}
