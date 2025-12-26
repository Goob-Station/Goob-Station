using Content.Shared.Actions;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Slasher.Components;

/// <summary>
/// Added to the Slasher user to track status.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SlasherMassacreUserComponent : Component
{
    /// <summary>
    /// Currently chained victim being massacred. Reset if miss or victim changes.
    /// </summary>
    [ViewVariables] public EntityUid? CurrentVictim;

    /// <summary>
    /// Number of consecutive successful hits in current chain.
    /// </summary>
    [ViewVariables] public int HitCount;

    /// <summary>
    /// Whether massacre mode is active.
    /// </summary>
    [ViewVariables] public bool Active;

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
    /// On reaching this many hits start attempting random limb severs each hit (10 by design).
    /// </summary>
    [DataField] public int LimbSeverHits = 4;

    /// <summary>
    /// Chance to sever a random limb.
    /// </summary>
    [DataField] public float LimbSeverChance = 0.40f;

    /// <summary>
    /// How many hits to decapitate
    /// </summary>
    [DataField] public int DecapitateHit = 13;

    [DataField] public EntProtoId MassacreActionId = "ActionSlasherMassacre";

    [ViewVariables] public EntityUid? MassacreActionEntity;

    [DataField]
    public SoundSpecifier MassacreIntro =
             new SoundPathSpecifier("/Audio/_Goobstation/Effects/Slasher/MassacreIntro.ogg")
             {
                 Params = AudioParams.Default
                     .WithRolloffFactor(8f)
                     .WithMaxDistance(10f)
             };

    [DataField]
    public SoundSpecifier MassacreSlash =
             new SoundPathSpecifier("/Audio/_Goobstation/Effects/Slasher/MassacreSlash.ogg")
             {
                 Params = AudioParams.Default
                     .WithRolloffFactor(8f)
                     .WithMaxDistance(10f)
             };

    [DataField]
    public SoundSpecifier MassacreDelimb =
             new SoundPathSpecifier("/Audio/_Goobstation/Effects/Slasher/MassacreDelimb.ogg")
             {
                 Params = AudioParams.Default
                     .WithRolloffFactor(8f)
                     .WithMaxDistance(10f)
             };
}
