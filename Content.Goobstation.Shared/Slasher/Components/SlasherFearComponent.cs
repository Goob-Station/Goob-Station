using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Alert;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Markdown.Mapping;

namespace Content.Goobstation.Shared.Slasher.Components;

/// <summary>
/// Drives the Slasher's fear meter.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SlasherFearComponent : Component
{
    /// <summary>
    /// Current fear charge.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Meter;

    [DataField]
    public float MaxMeter = 100f;

    [DataField]
    public float Range = 20f;

    /// <summary>
    /// How often the line-of-sight scan runs.
    /// </summary>
    [DataField]
    public TimeSpan CheckInterval = TimeSpan.FromSeconds(0.3);

    [DataField]
    public float MeterPerJumpscare = 7f;

    /// <summary>
    /// Meter gained per second while at least one victim is in sight.
    /// </summary>
    [DataField]
    public float MeterPassivePerSecond = 1.5f;

    [DataField]
    public float MeterDecayPerSecond = 2f;

    /// <summary>
    /// Grace period after the Slasher last had a victim in sight before the meter starts decaying.
    /// </summary>
    [DataField]
    public TimeSpan MeterGracePeriod = TimeSpan.FromSeconds(5);

    [DataField]
    public TimeSpan JumpscareCooldown = TimeSpan.FromSeconds(15);

    [DataField]
    public SoundSpecifier JumpscareSound =
        new SoundPathSpecifier("/Audio/_Goobstation/Slasher/Effects/Jumpscare1.ogg")
        {
            Params = AudioParams.Default.WithVolume(2f),
        };

    /// <summary>
    /// Immediate fear added to a victim when they are jumpscared.
    /// </summary>
    [DataField]
    public float JumpscareFear = 0.2f;

    /// <summary>
    /// Movement speed bonus granted to the Slasher at full meter.
    /// </summary>
    [DataField]
    public float MaxSpeedBonus = 0.15f;

    /// <summary>
    /// Meter value at which the slasher starts leaving a blood trail.
    /// </summary>
    [DataField]
    public float BloodMeterThreshold = 35f;

    [DataField]
    public ProtoId<AlertPrototype> Alert = "SlasherFear";

    /// <summary>
    /// Whether anyone currently has line of sight on the Slasher.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsObserved;

    [DataField]
    public ProtoId<AlertPrototype> SeenAlert = "SlasherSeen";

    /// <summary>
    /// Whether blood is currently being spilled.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsActive;

    [DataField, AutoNetworkedField]
    public bool SpeedBoostActive;

    /// <summary>
    /// Time between blood drops.
    /// </summary>
    [DataField]
    public TimeSpan DropInterval = TimeSpan.FromSeconds(0.2);

    /// <summary>
    /// Amount of blood spilled per drop.
    /// </summary>
    [DataField]
    public FixedPoint2 VolumePerDrop = FixedPoint2.New(1f);

    [DataField, AutoNetworkedField]
    public string BloodTrailReagent = "Blood";


    [DataField, AutoNetworkedField]
    public SoundSpecifier BloodTrailMusic =
               new SoundPathSpecifier("/Audio/_Goobstation/Slasher/Music/slasher_serial_killer_murder_frenzy_insane_horror_soundtrack.ogg")
               {
                   Params = AudioParams.Default
                       .WithVolume(-2f)
                       .WithRolloffFactor(8f)
                       .WithMaxDistance(10f)
                       .WithLoop(true)
               };

    /// <summary>
    /// How long in seconds the theme takes to fade to silence when the hunt ends.
    /// </summary>
    [DataField]
    public float MusicFadeDuration = 5f;

    /// <summary>
    /// Volume (in dB) treated as silent - the fade drives the theme down to this before stopping it.
    /// </summary>
    [DataField]
    public float MusicSilentVolume = -32f;

    [DataField]
    public ComponentRegistry FearStyle = new()
    {
        { "SlasherFearOverlay", new EntityPrototype.ComponentRegistryEntry(new SlasherFearOverlayComponent(), new MappingDataNode()) },
    };

    /// <summary>
    /// The status effect applied to victims this slasher frightens.
    /// </summary>
    [DataField]
    public EntProtoId FearedEffect = "StatusEffectFeared";

    [DataField, AutoNetworkedField]
    public List<SoundSpecifier> JumpscareSounds = new()
    {
        new SoundPathSpecifier("/Audio/_Goobstation/Slasher/Effects/Jumpscare1.ogg"),
        new SoundPathSpecifier("/Audio/_Goobstation/Slasher/Effects/Jumpscare2.ogg"),
        new SoundPathSpecifier("/Audio/_Goobstation/Slasher/Effects/Jumpscare3.ogg"),
        new SoundPathSpecifier("/Audio/_Goobstation/Slasher/Effects/Jumpscare4.ogg")
    };

    [DataField, AutoNetworkedField]
    public TimeSpan NextCheck;

    /// <summary>
    /// Victims that were in sight on the previous scan, to detect fresh entries.
    /// </summary>
    [DataField, AutoNetworkedField]
    public HashSet<EntityUid> Observing = new();

    [DataField, AutoNetworkedField]
    public TimeSpan NextJumpscare;

    [DataField, AutoNetworkedField]
    public TimeSpan LastSeenVictim;

    [DataField, AutoNetworkedField]
    public bool MusicActive;

    [ViewVariables]
    public EntityUid? MusicStream;
}
