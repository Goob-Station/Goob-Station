using Content.Shared.Damage;
using Robust.Shared.Audio;

namespace Content.Server._Lavaland.Mobs.Hierophant.Components;

[RegisterComponent]
public sealed partial class HierophantDamageFieldComponent : Component
{
    [DataField]
    public DamageSpecifier Damage = default!;

    [DataField]
    public SoundPathSpecifier? Sound;
}
