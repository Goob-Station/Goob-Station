using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Slasher.Components;

/// <summary>
/// Creates blood at the entities feet and plays some music. Originally for the slasher.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class SlasherBloodTrailComponent : Component
{
    [ViewVariables]
    public EntityUid? ActionEntity;

    [DataField]
    public EntProtoId ActionId = "ActionSlasherBloodTrail";

    /// <summary>
    /// Whether the trail is currently enabled.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsActive;

    /// <summary>
    /// Time between drops.
    /// </summary>
    [DataField]
    public TimeSpan DropInterval = TimeSpan.FromSeconds(0.2);

    /// <summary>
    /// Amount of blood spilled per drop.
    /// </summary>
    [DataField]
    public FixedPoint2 VolumePerDrop = FixedPoint2.New(1f);

    [DataField]
    public SoundSpecifier Funkyslasher =
               new SoundPathSpecifier("/Audio/_Goobstation/Music/slasher_serial_killer_murder_frenzy_insane_horror_soundtrack.ogg")
               {
                   Params = AudioParams.Default
                       .WithVolume(-2f)
                       .WithRolloffFactor(8f)
                       .WithMaxDistance(10f)
                       .WithLoop(true)
               };

    [ViewVariables]
    public EntityUid? FunkyslasherStream;
}
