using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Curses;

[RegisterComponent, NetworkedComponent]
public sealed partial class CurseDeathComponent : Component
{
    [DataField]
    public EntProtoId SmokeProto = "AdminInstantEffectSmoke3";

    [DataField]
    public int TicksElapsed;

    [DataField]
    public float RampMultiplier;

    [DataField]
    public DamageSpecifier BaseDamage = new();

    [DataField]
    public ProtoId<ReagentPrototype> Reagent = "Ammonia";

    [DataField]
    public float SmokeDuration = 7f;

    [DataField]
    public int SmokeSpread = 18;

    [DataField]
    public FixedPoint2 WpGeneration = 2;

    [DataField]
    public ProtoId<CursePrototype> Curse = "CurseDeath";

    [DataField]
    public SoundSpecifier? CurseSound1 = new SoundPathSpecifier("/Audio/_Goobstation/Wraith/Ambience/Void_Wail.ogg");

    [DataField]
    public bool MusicIsPlaying;

    [DataField]
    public bool EndIsNigh;

    [DataField]
    public SoundSpecifier? CurseSound2 = new SoundPathSpecifier("/Audio/_Goobstation/Wraith/wraithwhisper2.ogg");
}
