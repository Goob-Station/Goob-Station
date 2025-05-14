using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._DV.CosmicCult.Components;

/// <summary>
/// Component for Cosmic Cult's Vacuous Chantry.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentPause]
public sealed partial class CosmicChantryComponent : Component
{
    [DataField]
    public EntityUid PolyVictim;

    [DataField]
    public EntityUid Victim;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan SpawnTimer = default!;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan CountdownTimer = default!;

    [DataField]
    public bool Spawned;

    [DataField]
    public bool Completed;

    [DataField]
    public SoundSpecifier ChantryAlarm = new SoundPathSpecifier("/Audio/_DV/CosmicCult/chantry_alarm.ogg");

    [DataField]
    public EntProtoId Colossus = "MobCosmicColossus";

    [DataField]
    public EntProtoId SpawnVFX = "CosmicGlareAbilityVFX";
}

[Serializable, NetSerializable]
public enum ChantryVisuals : byte
{
    Status,
}

[Serializable, NetSerializable]
public enum ChantryStatus : byte
{
    Off,
    On,
}
