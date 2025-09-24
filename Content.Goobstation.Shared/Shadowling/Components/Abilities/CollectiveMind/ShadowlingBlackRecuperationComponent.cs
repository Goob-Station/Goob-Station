using Content.Shared.Humanoid.Markings;
using Content.Shared.Polymorph;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Shadowling.Components.Abilities.CollectiveMind;

/// <summary>
/// This is used for the Black Recuperation ability.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingBlackRecuperationComponent : Component
{
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(5);

    /// <summary>
    /// The effect that is used once the ability activates.
    /// </summary>
    [DataField]
    public EntProtoId BlackRecuperationEffect = "ShadowlingBlackRecuperationEffect";

    /// <summary>
    /// The max limit of Lesser Shadowlings that a Shadowling can have.
    /// </summary>
    [DataField]
    public int LesserShadowlingMaxLimit = 5;

    /// <summary>
    /// The current amount of Lesser Shadowlings that the Shadowling has.
    /// </summary>
    [DataField]
    public int LesserShadowlingAmount;

    /// <summary>
    /// The polymorph species of the Lesser Shadowlings
    /// </summary>
    [DataField]
    public ProtoId<PolymorphPrototype> LesserShadowlingSpeciesProto = "ShadowPolymorph";

    /// <summary>
    /// The marking of the eyes of a Lesser Shadowling.
    /// </summary>
    [DataField]
    public ProtoId<MarkingPrototype> MarkingId = "LesserShadowlingEyes";

    /// <summary>
    /// The sound that is used once the ability activates.
    /// </summary>
    [DataField]
    public SoundSpecifier? BlackRecSound = new SoundPathSpecifier("/Audio/Items/Defib/defib_zap.ogg");

    /// <summary>
    /// How much light resistance the ability removes from the Shadowling, if used on a dead Thrall
    /// </summary>
    [DataField]
    public float ResistanceRemoveFromThralls = 0.5f;

    /// <summary>
    /// How much light resistance the ability removes from the Shadowling, if used to convert a Thrall to a Lesser Shadowling
    /// </summary>
    [DataField]
    public float ResistanceRemoveFromLesser = 0.12f;

    [DataField]
    public EntProtoId LesserSlingComponents = "LesserShadowlingAbilities";

    [DataField]
    public EntProtoId ActionId = "ActionBlackRecuperation";

    [DataField]
    public EntityUid? ActionEnt;
}
