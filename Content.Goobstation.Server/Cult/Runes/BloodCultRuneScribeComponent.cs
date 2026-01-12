using Content.Shared.Damage;
using Robust.Shared.Audio;

namespace Content.Goobstation.Server.Cult.Runes;

[RegisterComponent]
public sealed partial class BloodCultRuneScribeComponent : Component
{
    [DataField] public DamageSpecifier DamagePerScribe = new();

    [DataField] public SoundSpecifier? ScribeSound;

    [DataField] public SoundSpecifier? RuneDestroySound;
}
