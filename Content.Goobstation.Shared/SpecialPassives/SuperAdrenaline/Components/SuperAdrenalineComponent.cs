using Content.Shared.Alert;
using Content.Shared.Damage;
using Content.Shared.Mobs;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.SpecialPassives.SuperAdrenaline.Components;

/// <summary>
///     Entities with this cannot be incapacitated by normal means. This component holds all the relevant data.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SuperAdrenalineComponent : Component
{
    /// <summary>
    /// The alert id of the component (if one should exist)
    /// </summary>
    public ProtoId<AlertPrototype>? AlertId;

    /// <summary>
    /// How long should the effect go on for?
    /// </summary>
    [DataField]
    public float? Duration;

    public TimeSpan MaxDuration = TimeSpan.Zero;

    public TimeSpan UpdateTimer = default!;

    /// <summary>
    /// Delay between stamina regeneration ticks.
    /// </summary>
    public TimeSpan UpdateDelay = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Current mobstate of the entity.
    /// </summary>
    public MobState Mobstate;

    /// <summary>
    /// Should the entity ignore knockdown attempts?
    /// </summary>
    [DataField]
    public bool IgnoreKnockdown = true;

    /// <summary>
    /// Should the entity ignore stun attempts?
    /// </summary>
    [DataField]
    public bool IgnoreStun = true;

    /// <summary>
    /// Should the entity ignore sleep attempts?
    /// </summary>
    [DataField]
    public bool IgnoreSleep = true;

    /// <summary>
    /// Should the entity ignore slowdown from stun?
    /// </summary>
    [DataField]
    public bool IgnoreStunSlowdown = true;

    /// <summary>
    /// Should the entity ignore slowdown from damage?
    /// </summary>
    [DataField]
    public bool IgnoreDamageSlowdown = true;

    /// <summary>
    /// Should the entity ignore pain?
    /// </summary>
    [DataField]
    public bool IgnorePain = true;

    /// <summary>
    /// The amount of stamina the entity should regenerate per tick.
    /// </summary>
    [DataField]
    public float StaminaRegeneration = 10f;

    /// <summary>
    /// The damage the entity should passively take from having the status effect.
    /// </summary>
    [DataField]
    public DamageSpecifier? PassiveDamage;
}
