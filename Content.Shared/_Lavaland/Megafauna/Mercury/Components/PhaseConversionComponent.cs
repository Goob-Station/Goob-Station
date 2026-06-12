using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._Lavaland.Megafauna.Mercury.Components;

/// <summary>
/// Switches between the ranged form, and the melee form.
/// </summary>

[RegisterComponent, NetworkedComponent]
public sealed partial class PhaseConversionComponent : Component
{
    /// <summary>
    /// Sound played as it switches.
    /// </summary>
    [DataField]
    public SoundSpecifier SwitchSound = new SoundPathSpecifier("/Audio/_Lavaland/Mobs/Bosses/Mercury/conversion.ogg");

    /// <summary>
    /// Melee sprite.
    /// </summary>
    [DataField]
    public SpriteSpecifier MeleeSprite =
    new SpriteSpecifier.Rsi(new ResPath("_Lavaland/Mobs/Bosses/96x96.rsi"), "adapted_melee");

    /// <summary>
    /// Ranged sprite.
    /// </summary>
    [DataField]
    public SpriteSpecifier RangedSprite =
    new SpriteSpecifier.Rsi(new ResPath("_Lavaland/Mobs/Bosses/96x96.rsi"), "adapted_ranged");

    /// <summary>
    /// Melee action selector which tells which actions it can use.
    /// </summary>
    [DataField]
    public ProtoId<MegafaunaSelectorPrototype> MeleeSelector = "ORTMeleeActions";

    /// <summary>
    /// Ranged action selector which tells which actions it can use.
    /// </summary>
    [DataField]
    public ProtoId<MegafaunaSelectorPrototype> RangedSelector = "ORTRangedActions";

    /// <summary>
    /// Prototype spawned for cool.
    /// </summary>
    [DataField]
    public EntProtoId EffectPrototype = "ORTPhaseConversionEffect";
    public EntityUid? EffectEntity;

    /// <summary>
    /// Small delay between action usage and actual switch.
    /// </summary>
    [DataField]
    public float SwitchDelay = 1f;

    public bool SwitchSoon;
    public bool IsRanged = true;
    public float Accumulator;
}
[NetSerializable, Serializable]
public enum PhaseConversionVisuals : byte
{
    IsRanged,
}
