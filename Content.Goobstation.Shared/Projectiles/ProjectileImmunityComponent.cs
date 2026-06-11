using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Projectiles;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class ProjectileImmunityComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan? ExpireTime;

    [DataField, AutoNetworkedField]
    public EntProtoId? DodgeEffect;

    [DataField, AutoNetworkedField]
    public float StaminaCostPerDodge;

    [DataField, AutoNetworkedField]
    public float BatteryCostPerDodge;

    public HashSet<EntityUid> DodgedEntities = new();
}

[ByRefEvent]
public record struct ProjectileDodgeAttemptEvent(bool Cancelled = false);
