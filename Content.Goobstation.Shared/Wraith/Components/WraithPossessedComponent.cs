using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Components;

/// <summary>
/// Marks the entity as possessed by another entity.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class WraithPossessedComponent : Component
{
    /// <summary>
    /// The possessor of the entity
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Possessor;

    /// <summary>
    /// The possessor's mind
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? PossessorMind;

    /// <summary>
    /// The original mind
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? OriginalMind;

    /// <summary>
    /// How long to stay in the possessed entity
    /// </summary>
    [DataField]
    public TimeSpan PossessionDuration = TimeSpan.FromSeconds(30f);

    [ViewVariables, AutoNetworkedField]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    [DataField, AutoNetworkedField]
    public bool CancelEarly;

    [DataField]
    public DamageSpecifier RevenantDamageOvertime;
}

/// <summary>
///  Raised when the possession starts
/// </summary>
[ByRefEvent]
public record struct PossessionStartedEvent;

/// <summary>
///  Raised when the possession ends
/// </summary>
[ByRefEvent]
public record struct PossessionEndedEvent;

/// <summary>
///  Raised when possessing a fresh corpse as a Wraith
/// </summary>
/// <param name="Possessor"></param> The wraith
[ByRefEvent]
public record struct RevenantPossessedEvent(EntityUid? Possessor);
