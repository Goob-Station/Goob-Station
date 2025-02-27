using Content.Shared.Damage;
using Robust.Shared.Audio;

namespace Content.Server.Medical.CPR;

[RegisterComponent]
public sealed partial class CPRTrainingComponent : Component
{
    [DataField]
    public SoundSpecifier CPRSound = new SoundPathSpecifier("/Audio/Effects/CPR.ogg");

    [DataField]
    public TimeSpan DoAfterDuration = TimeSpan.FromSeconds(3);

    [DataField] public DamageSpecifier CPRHealing = new()
    {
        DamageDict =
        {
            ["Asphyxiation"] = -6
        }
    };

    [DataField] public float CrackRibsModifier = 1f;

    [DataField] public float ResuscitationChance = 0.1f;

    [DataField] public float RotReductionMultiplier;

    public EntityUid? CPRPlayingStream;
}
