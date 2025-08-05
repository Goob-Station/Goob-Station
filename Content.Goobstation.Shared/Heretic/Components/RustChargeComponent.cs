using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Heretic.Components;

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
    public SoundSpecifier HitSound = new SoundCollectionSpecifier("MetalSlam");

    [DataField]
    public List<EntityUid> DamagedEntities = [];

    [DataField]
    public TimeSpan KnockdownTime = TimeSpan.FromSeconds(2);
}
