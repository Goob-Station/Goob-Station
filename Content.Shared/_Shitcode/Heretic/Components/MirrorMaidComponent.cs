using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class MirrorMaidComponent : Component
{
    [DataField]
    public DamageSpecifier ExamineDamage = new()
    {
        DamageDict =
        {
            { "Blunt", 15 },
        }
    };

    [DataField]
    public TimeSpan ExamineDelay = TimeSpan.FromSeconds(5);

    [DataField]
    public EntProtoId ExamineStatus = "ExaminedMirrorMaidStatusEffect";

    [DataField]
    public SoundSpecifier? ExamineSound = new SoundPathSpecifier("/Audio/_Goobstation/Wizard/ghost2.ogg");
}
