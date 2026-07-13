using Content.Pirate.Shared.Avali.EntitySystems;
using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Pirate.Shared.Avali.Components;

/// <summary>
/// Allows an entity to enter and exit nanite-induced stasis.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause,
 Access(typeof(SharedStasisSystem))]
public sealed partial class StasisComponent : Component
{
    /// <summary>
    /// Whether the entity is currently in stasis.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsInStasis;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoNetworkedField, AutoPausedField]
    public TimeSpan NextHeal = TimeSpan.Zero;

    [DataField, AutoNetworkedField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Whether the entity should be visible. This is synced for correct PVS handling.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsVisible = true;

    /// <summary>
    /// The action granted to enter stasis.
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public EntProtoId EnterStasisAction;

    /// <summary>
    /// The action granted to exit stasis.
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public EntProtoId ExitStasisAction;

    [DataField, AutoNetworkedField]
    public EntityUid? ExitStasisActionEntity;

    [DataField, AutoNetworkedField]
    public EntityUid? EnterStasisActionEntity;

    /// <summary>
    /// The cooldown time for entering stasis.
    /// </summary>
    [DataField]
    public TimeSpan StasisCooldown = TimeSpan.FromSeconds(300);

    /// <summary>
    /// Damage healed per update interval.
    /// </summary>
    [DataField, AutoNetworkedField]
    public DamageSpecifier HealingPerUpdate = new();

    /// <summary>
    /// Bleed healed per update interval.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float BleedHealPerUpdate = 1.0f;

    /// <summary>
    /// Multiplier applied to healing while critical.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float CritHealingModifier = 2.0f;

    /// <summary>
    /// Multiplier applied to positive incoming damage while in stasis.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float StasisDamageReduction = 0.5f;

    /// <summary>
    /// The effect spawned while entering stasis.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntProtoId StasisEnterEffect = "EffectNanitesEnter";

    /// <summary>
    /// The delay between preparing and entering stasis.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan StasisEnterEffectLifetime = TimeSpan.FromSeconds(1.7);

    /// <summary>
    /// The sound played when entering stasis.
    /// </summary>
    [DataField, AutoNetworkedField]
    public SoundSpecifier StasisEnterSound = new SoundPathSpecifier("/Audio/_Pirate/Misc/alien_teleport.ogg");

    /// <summary>
    /// The effect spawned while exiting stasis.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntProtoId StasisExitEffect = "EffectNanitesExit";

    /// <summary>
    /// The lifetime of the effect spawned while exiting stasis, in seconds.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float StasisExitEffectLifetime = 1.7f;

    /// <summary>
    /// The sound played when exiting stasis.
    /// </summary>
    [DataField, AutoNetworkedField]
    public SoundSpecifier StasisExitSound = new SoundPathSpecifier("/Audio/_Pirate/Misc/alien_teleport.ogg");

    /// <summary>
    /// The effect displayed while stasis is active.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntProtoId StasisContinuousEffect = "EffectNanitesCurrent";

    /// <summary>
    /// Server-side reference to the continuous stasis effect.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? ContinuousEffectEntity;

    /// <summary>
    /// Client-side reference to the continuous stasis effect.
    /// </summary>
    [DataField]
    public EntityUid? ClientContinuousEffectEntity;

    /// <summary>
    /// Client-side reference to the prepare-stasis effect.
    /// </summary>
    [DataField]
    public EntityUid? ClientEnterEffectEntity;
}
