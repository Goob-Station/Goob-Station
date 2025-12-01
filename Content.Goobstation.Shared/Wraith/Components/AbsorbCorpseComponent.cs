using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Damage;
using Content.Shared.Tag;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Components;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class AbsorbCorpseComponent : Component
{
    /// <summary>
    /// The amount of corpses that have already been absorbed
    /// </summary>
    [DataField, AutoNetworkedField]
    public int CorpsesAbsorbed;

    [DataField]
    public EntProtoId SmokeProto = "AdminInstantEffectSmoke3";

    /// <summary>
    /// How much to add to the generation rate of WP of the entity
    /// </summary>
    [DataField]
    public FixedPoint2 WpPassiveAdd = 0.4;

    /// <summary>
    /// If the target has this much of X chem in their body, it hurts the wraith.
    /// </summary>
    [DataField]
    public FixedPoint2 FormaldehydeThreshhold = 25;

    /// <summary>
    /// Removes all the formaldehyde from their system.
    /// </summary>
    [DataField]
    public FixedPoint2 ChemToRemove = 999;

    /// <summary>
    ///  Damage to deal to the wraith.
    /// </summary>
    [DataField]
    public DamageSpecifier Damage = new();

    [DataField]
    public ProtoId<ReagentPrototype> Reagent = "Formaldehyde";

    /// <summary>
    /// Sounds to be played when wraith absorbs someone.
    /// </summary>
    [DataField]
    public SoundSpecifier? AbsorbSound = new SoundCollectionSpecifier("Wraith_SoulSucc");

    [ViewVariables]
    public ProtoId<TagPrototype> Tag = "VimPilot";
}

/// <summary>
/// Raised on user once trying to absorb a corpse
/// </summary>
[ByRefEvent]
public record struct AbsorbCorpseAttemptEvent(EntityUid Target, bool Handled = false, bool Cancelled = false);
