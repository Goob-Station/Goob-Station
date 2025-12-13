using Content.Shared.Alert;
using Content.Shared.Damage;
using Content.Shared.Mobs;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.SpecialPassives.SuperAdrenaline.Components;

/// <summary>
///     Entities with this cannot be incapacitated by normal means. This component holds all the relevant data.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true), AutoGenerateComponentPause]
public sealed partial class SuperAdrenalineComponent : Component
{
    /// <summary>
    /// The alert id of the component (if one should exist)
    /// </summary>
    [DataField]
    public ProtoId<AlertPrototype>? AlertId;

    /// <summary>
    /// How long should the effect go on for?
    /// </summary>
    [DataField, AutoNetworkedField]
    public float? Duration;

    [DataField, AutoNetworkedField]
    public TimeSpan MaxDuration = TimeSpan.Zero;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan UpdateTimer = default!;

    /// <summary>
    /// Delay between stamina regeneration ticks.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan UpdateDelay = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Current mobstate of the entity.
    /// </summary>
    [DataField, AutoNetworkedField]
    public MobState Mobstate;

    /// <summary>
    /// Should the entity ignore knockdown attempts?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IgnoreKnockdown = true;

    /// <summary>
    /// Should the entity ignore stun attempts?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IgnoreStun = true;

    /// <summary>
    /// Should the entity ignore sleep attempts?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IgnoreSleep = true;

    /// <summary>
    /// Should the entity ignore slowdown from stun?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IgnoreStunSlowdown = true;

    /// <summary>
    /// Should the entity ignore slowdown from damage?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IgnoreDamageSlowdown = true;

    /// <summary>
    /// Should the entity ignore pain?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IgnorePain = true;

    /// <summary>
    /// The amount of stamina the entity should regenerate per tick.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float StaminaRegeneration = 10f;

    /// <summary>
    /// The damage the entity should passively take from having the status effect.
    /// </summary>
    [DataField, AutoNetworkedField]
    public DamageSpecifier? PassiveDamage;
}
