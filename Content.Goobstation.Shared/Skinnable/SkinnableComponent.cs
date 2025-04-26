using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;

namespace Content.Goobstation.Shared.Skinnable;

[RegisterComponent]
public sealed partial class SkinnableComponent : Component
{
    [DataField]
    public bool Skinned;

    [DataField]
    public TimeSpan SkinningDoAfterDuation = TimeSpan.FromSeconds(5);

    [DataField]
    public DamageSpecifier DamageOnSkinned = new() { DamageDict = new Dictionary<string, FixedPoint2> { { "Slash", 50 } } };

    [DataField]
    public SoundSpecifier SkinSound = new SoundPathSpecifier("/Audio/_Shitmed/Medical/Surgery/scalpel1.ogg");
}
