using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.AlmanacBlade;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AlmanacBladeComponent : Component
{
    [DataField]
    public float NewMoonAttackRate = 0.75f;

    [DataField]
    public float FullMoonAttackRate = 1f;

    [DataField]
    public float EclipseAttackRate = 10f;

    [DataField]
    public DamageSpecifier ColdDamage = new() { DamageDict = new() { { "Cold", 5 } } };

    [DataField]
    public float FireStacks = 1f;

    [DataField]
    public EntProtoId Present = "PresentRandom";

    [DataField]
    public TimeSpan PresentCooldown = TimeSpan.FromMinutes(3);

    [DataField]
    public TimeSpan NextPresent;

    [DataField]
    public SoundSpecifier ChristmasSound = new SoundPathSpecifier("/Audio/Effects/hallelujah.ogg");

    [DataField]
    public SoundSpecifier HardBass = new SoundPathSpecifier("/Audio/_Goobstation/RadioStation/music/PMMUSIC/Russian-Dance-PM-Music.ogg");

    [DataField]
    public float ExplosionIntensity = 40f;

    [ViewVariables]
    public EntityUid? HardBassStream;

    [DataField]
    public bool FreezeCalendar;

    [DataField, AutoNetworkedField]
    public bool LeapDay;

    [DataField, AutoNetworkedField]
    public bool Afternoon;

    [DataField, AutoNetworkedField]
    public bool Christmas;

    [DataField, AutoNetworkedField]
    public bool HardBassDay;

    [DataField, AutoNetworkedField]
    public bool WednesdayBeforeChristmas;

    [DataField, AutoNetworkedField]
    public bool MondayBeforeGarfield;

    [DataField, AutoNetworkedField]
    public bool AprilFoolsOver;
}
