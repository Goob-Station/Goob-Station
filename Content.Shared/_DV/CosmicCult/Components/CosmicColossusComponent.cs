using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._DV.CosmicCult.Components;

/// <summary>
/// Component for Cosmic Cult's entropic colossus.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentPause]
public sealed partial class CosmicColossusComponent : Component
{
    [AutoPausedField, DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan AttackHoldTimer = default!;

    [DataField] public SoundSpecifier DeathSfx = new SoundPathSpecifier("/Audio/_DV/CosmicCult/colossus_death.ogg");

    [DataField] public SoundSpecifier IngressSfx = new SoundPathSpecifier("/Audio/_DV/CosmicCult/ability_ingress.ogg");

    [DataField] public SoundSpecifier DoAfterSfx = new SoundPathSpecifier("/Audio/Machines/airlock_creaking.ogg");

    [DataField] public EntProtoId CultVfx = "CosmicGenericVFX";

    [DataField] public EntProtoId Attack1Vfx = "CosmicColossusAttack1Vfx";

    [DataField] public EntProtoId TileDetonations = "MobTileDamageZone";

    [DataField] public TimeSpan IngressDoAfter = TimeSpan.FromSeconds(7);

    [DataField] public TimeSpan AttackWait = TimeSpan.FromSeconds(1.5);

    [DataField] public bool Attacking;
}

[Serializable, NetSerializable]
public enum ColossusVisuals : byte
{
    Status,
}

[Serializable, NetSerializable]
public enum ColossusStatus : byte
{
    Alive,
    Dead,
    Attacking,
}
