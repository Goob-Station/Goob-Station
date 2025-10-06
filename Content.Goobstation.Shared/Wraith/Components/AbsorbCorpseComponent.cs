using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Components;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class AbsorbCorpseComponent : Component
{
    /// <summary>
    /// The amount of time the doafter takes for the Wraith to absorb a corpse. Original system doesn't use a do after but I'm too lazy to rework it.
    /// </summary>
    [DataField]
    public TimeSpan AbsorbDuration = TimeSpan.FromSeconds(0.1);

    /// <summary>
    /// The amount of corpses that have already been absorbed
    /// </summary>
    [DataField, AutoNetworkedField]
    public int CorpsesAbsorbed;

    [DataField]
    public EntProtoId SmokeProto = "AdminInstantEffectSmoke10";

    /// <summary>
    /// How much to add to the generation rate of WP of the entity
    /// </summary>
    [DataField]
    public FixedPoint2 WpPassiveAdd = 0.4;

    /// <summary>
    /// Sounds to be played whwn wraith absorbs someone.
    /// </summary>
    [DataField]
    public SoundSpecifier? AbsorbSound = new SoundCollectionSpecifier("Wraith_SoulSucc");
}

[ByRefEvent]
public record struct AbsorbCorpseAttemptEvent(
    EntityUid User,
    EntityUid Target,
    bool Cancelled = false,
    bool Handled = false);
