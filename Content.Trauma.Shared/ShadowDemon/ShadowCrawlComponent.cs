using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Trauma.Shared.ShadowDemon;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class ShadowCrawlComponent : Component
{
    /// <summary>
    /// The phase in effect when you start the jaunt
    /// </summary>
    [DataField]
    public EntProtoId PhaseIn;

    /// <summary>
    /// The phase out effect when you start the jaunt
    /// </summary>
    [DataField]
    public EntProtoId PhaseOut;

    /// <summary>
    /// How much damage to reduce from lights while in jaunt
    /// </summary>
    [DataField]
    public float DamageModiferFromLights = 0.5f;

    /// <summary>
    /// How much speed boost to get while in jaunt
    /// </summary>
    [DataField]
    public float SpeedBuff = 8;

    /// <summary>
    /// The cooldown applied to the action when exiting the jaunt
    /// </summary>
    [DataField]
    public TimeSpan ActionCooldown = TimeSpan.FromSeconds(4);

    /// <summary>
    /// The cooldown applied to the action when shooting a grapple
    /// </summary>
    [DataField]
    public TimeSpan ActionCooldownAfterGrapple = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Whether we are in jaunt or not
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public bool Active;

    [DataField]
    public EntProtoId ActionId = "ShadowCrawlAction";

    [ViewVariables, AutoNetworkedField]
    public EntityUid? ActionUid;
}
