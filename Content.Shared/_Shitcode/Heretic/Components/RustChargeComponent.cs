using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitcode.Heretic.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
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

    [DataField, AutoPausedField]
    public TimeSpan NextRustTime = TimeSpan.Zero;

    [DataField]
    public TimeSpan RustPeriod = TimeSpan.FromSeconds(0.1);

    [DataField]
    public List<EntityUid> DamagedEntities = new();

    [DataField]
    public TimeSpan KnockdownTime = TimeSpan.FromSeconds(5);

    [DataField]
    public float RustRadius = 1.5f;

    [DataField]
    public float LookupRange = 0.1f;

    [DataField]
    public EntProtoId TileRune = "TileHereticRustRune";
}
