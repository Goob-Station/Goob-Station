using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Slasher.Components;

/// <summary>
/// Added to the Slasher user to track status.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SlasherMassacreUserComponent : Component
{
    /// <summary>
    /// Currently chained victim being massacred. Reset if miss or victim changes.
    /// </summary>
    [ViewVariables, AutoNetworkedField] public EntityUid? CurrentVictim;

    /// <summary>
    /// Number of consecutive successful hits in current chain.
    /// </summary>
    [ViewVariables, AutoNetworkedField] public int HitCount;

    /// <summary>
    /// Whether massacre mode is active.
    /// </summary>
    [ViewVariables, AutoNetworkedField] public bool Active;

    /// <summary>
    /// Time when the last attack occurred. Used for timeout tracking.
    /// </summary>
    [ViewVariables, AutoNetworkedField] public TimeSpan? LastAttackTime;

    /// <summary>
    /// How long before the chain automatically ends.
    /// </summary>
    [DataField] public float ChainTimeoutSeconds = 10f;

    [DataField]
    public SoundSpecifier MassacreIntro =
         new SoundPathSpecifier("/Audio/_Goobstation/Effects/Slasher/MassacreIntro.ogg")
         {
             Params = AudioParams.Default
                 .WithVolume(-5f)
                 .WithRolloffFactor(8f)
                 .WithMaxDistance(10f)
         };
}

/// <summary>
/// Temporarily added to victims being attacked during massacre mode.
/// Used to track when they die and apply kill bonuses.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SlasherMassacreVictimComponent : Component
{
    /// <summary>
    /// The attacker performing the massacre.
    /// </summary>
    [ViewVariables, AutoNetworkedField] public EntityUid? Attacker;

    /// <summary>
    /// Reference to the weapon component.
    /// </summary>
    [ViewVariables] public SlasherMassacreMacheteComponent? WeaponComp;

    /// <summary>
    /// The sound to play on kill.
    /// </summary>
    [DataField]
    public SoundSpecifier MassacreFinale =
             new SoundPathSpecifier("/Audio/_Goobstation/Effects/Slasher/MassacreEnd.ogg")
             {
                 Params = AudioParams.Default
                     .WithRolloffFactor(8f)
                     .WithMaxDistance(10f)
             };
}

/// <summary>
/// Added to the machete to give it the massacre action.
/// It's basic functionality is this:
/// Activate it, causing a damage penalty
/// every hit gives extra damage based on hit count
/// start severing limbs after a set number of hits
/// guranteed decapitation after enough hits
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SlasherMassacreMacheteComponent : Component
{
    /// <summary>
    /// Flat damage penalty applied on first hit while massacre active.
    /// </summary>
    [DataField] public int BaseDamagePenalty = 9;

    /// <summary>
    /// Bonus damage added per successful chained hit.
    /// </summary>
    [DataField] public int PerHitBonus = 3;

    /// <summary>
    /// On reaching this many hits sever a random limb. Limb is severed every multiple of this value.
    /// </summary>
    [DataField] public int LimbSeverHits = 4;

    /// <summary>
    /// How many hits to decapitate
    /// </summary>
    [DataField] public int DecapitateHit = 13;

    /// <summary>
    /// Speed bonus multiplier per hit when killing a target.
    /// </summary>
    [DataField] public float SpeedBonusPerHit = 0.01f;

    /// <summary>
    /// Duration of speed boost in seconds.
    /// </summary>
    [DataField] public float SpeedBoostDuration = 10f;

    /// <summary>
    /// Amount of healing reagent to inject per hit when killing a target.
    /// </summary>
    [DataField] public float HealAmountPerHit = 0.5f;

    /// <summary>
    /// The healing reagent to inject on kill.
    /// </summary>
    [DataField] public ProtoId<ReagentPrototype> HealReagent = "slasherium";

    /// <summary>
    /// The MassacreAction.
    /// </summary>
    [DataField] public EntProtoId MassacreActionId = "ActionSlasherMassacre";

    [ViewVariables] public EntityUid? MassacreActionEntity;

    /// <summary>
    /// Sound played when massacre is turned on.
    /// </summary>
    [DataField]
    public SoundSpecifier MassacreIntro =
             new SoundPathSpecifier("/Audio/_Goobstation/Effects/Slasher/MassacreIntro.ogg")
             {
                 Params = AudioParams.Default
                     .WithRolloffFactor(8f)
                     .WithMaxDistance(10f)
             };

    /// <summary>
    /// Sound to play when doing normal hits.
    /// </summary>
    [DataField]
    public SoundSpecifier MassacreSlash =
             new SoundPathSpecifier("/Audio/_Goobstation/Effects/Slasher/MassacreSlash.ogg")
             {
                 Params = AudioParams.Default
                     .WithRolloffFactor(8f)
                     .WithMaxDistance(10f)
             };

    /// <summary>
    /// Sound to play when delimbing.
    /// </summary>
    [DataField]
    public SoundSpecifier MassacreDelimb =
             new SoundPathSpecifier("/Audio/_Goobstation/Effects/Slasher/MassacreDelimb.ogg")
             {
                 Params = AudioParams.Default
                     .WithRolloffFactor(8f)
                     .WithMaxDistance(10f)
             };
}
