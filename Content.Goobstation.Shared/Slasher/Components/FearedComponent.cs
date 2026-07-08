using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Slasher.Components;

/// <summary>
/// Applied to anyone the Slasher has frightened. Fear rises while
/// the Slasher can see them and decays once they break line of sight.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FearedComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Fear;

    [ViewVariables, AutoNetworkedField]
    public EntityUid? Scarer;

    /// <summary>
    /// The status effect that granted this component.
    /// </summary>
    [ViewVariables]
    public EntProtoId SourceEffect;

    [ViewVariables]
    public ComponentRegistry? AppliedStyle;

    /// <summary>
    /// Fear gained per second while the Slasher has line of sight.
    /// </summary>
    [DataField]
    public float GainPerSecond = 0.02f;

    [DataField]
    public float DecayPerSecond = 0.03f;

    /// <summary>
    /// How long after a Slasher last had line of sight the victim still counts as observed.
    /// </summary>
    [DataField]
    public TimeSpan ObserveTimeout = TimeSpan.FromSeconds(0.6);

    /// <summary>
    /// Grace period after the Slasher loses sight before the victim's fear begins to fall.
    /// </summary>
    [DataField]
    public TimeSpan FearGracePeriod = TimeSpan.FromSeconds(5);

    /// <summary>
    /// How long after the owning Slasher last had the victim in sight before a
    /// different Slasher may claim them.
    /// </summary>
    [DataField]
    public TimeSpan OwnershipTimeout = TimeSpan.FromSeconds(1);

    /// <summary>
    /// At or above this fear, the victim is slowed.
    /// </summary>
    [DataField]
    public float SlowThreshold = 0.45f;

    /// <summary>
    /// Movement multiplier applied while slowed (walk and sprint).
    /// </summary>
    [DataField]
    public float SlowMultiplier = 0.7f;

    /// <summary>
    /// How long the slow is refreshed for on each update the victim stays above the slow threshold.
    /// </summary>
    [DataField]
    public TimeSpan SlowRefresh = TimeSpan.FromSeconds(1.5);

    [DataField]
    public EntProtoId SlowEffect = "SlasherSlowdownStatusEffect";

    /// <summary>
    /// At or above this fear, the victim takes periodic damage.
    /// </summary>
    [DataField]
    public float DamageThreshold = 0.9f;

    /// <summary>
    /// Damage applied each second while over the damage threshold.
    /// </summary>
    [DataField]
    public DamageSpecifier DamagePerSecond = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2> { { "Asphyxiation", 1.5 } },
    };

    /// <summary>
    /// Last time a Slasher had this victim in sight.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan LastObserved;

    /// <summary>
    /// How often this victim's fear is recalculated.
    /// </summary>
    [DataField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(1);

    [DataField, AutoNetworkedField]
    public TimeSpan NextUpdate;

    [ViewVariables]
    public EntityUid? MusicStream;

    [ViewVariables]
    public EntityUid? MusicScarer;
}
