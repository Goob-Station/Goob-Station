using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._DV.CosmicCult.Components;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentPause]
public sealed partial class CosmicGlyphComponent : Component
{
    [DataField]
    public int RequiredCultists = 1;

    [DataField]
    public float ActivationRange = 1.55f;

    /// <summary>
    ///     Damage dealt on glyph activation.
    /// </summary>
    [DataField] public DamageSpecifier ActivationDamage = new();
    [DataField] public bool CanBeErased = true;
    [DataField] public bool EraseOnUse = false;
    [DataField] public EntProtoId GlyphVFX = "CosmicGenericVFX";
    [DataField] public SoundSpecifier TriggerSFX = new SoundPathSpecifier("/Audio/_DV/CosmicCult/glyph_trigger.ogg");
    [DataField] public SoundSpecifier ChargeSFX = new SoundPathSpecifier("/Audio/_DV/CosmicCult/glyph_charge.ogg");
    [DataField] public SoundSpecifier FailSFX = new SoundPathSpecifier("/Audio/_DV/CosmicCult/glyph_fail.ogg");
    [DataField] public GlyphStatus State = GlyphStatus.Spawning;
    [DataField] public EntityUid? User = null;
    [DataField] public TimeSpan SpawnTime = TimeSpan.FromSeconds(1.2);
    [DataField] public TimeSpan DespawnTime = TimeSpan.FromSeconds(0.6);
    [DataField] public TimeSpan ActivationTime = TimeSpan.FromSeconds(0);
    [DataField] public TimeSpan CooldownTime = TimeSpan.FromSeconds(3.0);
    [AutoPausedField, DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan Timer = default!;
}

[ByRefEvent]
public record struct TryActivateGlyphEvent(EntityUid User, HashSet<Entity<CosmicCultComponent>> Cultists, bool Cancelled = false)
{
    public void Cancel()
    {
        Cancelled = true;
    }
}

[ByRefEvent]
public record struct CheckGlyphConditionsEvent(EntityUid User, HashSet<Entity<CosmicCultComponent>> Cultists, bool Cancelled = false)
{
    public void Cancel()
    {
        Cancelled = true;
    }
}

[Serializable, NetSerializable]
public enum GlyphVisuals : byte
{
    Status,
}

[Serializable, NetSerializable]
public enum GlyphStatus : byte
{
    Spawning,
    Despawning,
    Ready,
    Active,
    Cooldown
}
