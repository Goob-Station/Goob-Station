using Content.Shared.Damage;
using Content.Shared.Tag;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class RustChargeComponent : Component
{
    [DataField]
    public DamageSpecifier Damage = new()
    {
        DamageDict =
        {
            {"Blunt", 50f},
        },
    };

    [DataField]
    public ProtoId<TagPrototype> IgnoreTag = "IgnoreImmovableRod";

    [DataField]
    public SoundSpecifier HitSound = new SoundCollectionSpecifier("MetalSlam");

    [DataField]
    public List<EntityUid> DamagedEntities = new();

    [DataField]
    public TimeSpan KnockdownTime = TimeSpan.FromSeconds(2);
}
