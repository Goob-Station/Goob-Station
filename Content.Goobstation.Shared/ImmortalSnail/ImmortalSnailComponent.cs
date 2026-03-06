using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.ImmortalSnail;

/// <summary>
/// Component for the immortal snail entity that tracks its target.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ImmortalSnailComponent : Component
{
    /// <summary>
    /// The entity that the snail is hunting.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Target;

    /// <summary>
    /// The game rule entity that spawned this snail.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? RuleEntity;

    /// <summary>
    /// The Touch of Death action.
    /// </summary>
    [DataField]
    public EntProtoId TouchOfDeathAction = "ActionImmortalSnailTouchOfDeath";

    /// <summary>
    /// The spawned Touch of Death action entity.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? TouchOfDeathActionEntity;

    /// <summary>
    /// The Track Target (heartbeat) action.
    /// </summary>
    [DataField]
    public EntProtoId HeartbeatAction = "ActionImmortalSnailHeartbeat";

    /// <summary>
    /// The spawned Track Target action entity.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? HeartbeatActionEntity;
}
