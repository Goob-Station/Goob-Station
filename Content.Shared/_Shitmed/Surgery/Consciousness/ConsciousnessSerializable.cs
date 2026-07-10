using Content.Shared._Shitmed.Medical.Surgery.Pain;
using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.Serialization;

namespace Content.Shared._Shitmed.Medical.Surgery.Consciousness;

[Serializable]
public enum ConsciousnessModType
{
    Generic, // Same for generic
    Pain, // Pain is affected only by pain multipliers
}

// The networking on consciousness is rather silly.
[Serializable, NetSerializable]
public sealed class ConsciousnessComponentState : ComponentState
{
    public FixedPoint2 Threshold;
    public FixedPoint2 RawConsciousness;
    public FixedPoint2 Multiplier;
    public FixedPoint2 Cap;

    public readonly Dictionary<(NetEntity, string), ConsciousnessModifier> Modifiers;
    public readonly Dictionary<(NetEntity, string), ConsciousnessMultiplier> Multipliers;
    public readonly Dictionary<string, (NetEntity?, bool, bool)> RequiredConsciousnessParts;

    public ConsciousnessComponentState()
    {
        Modifiers = new();
        Multipliers = new();
        RequiredConsciousnessParts = new();
    }

    public ConsciousnessComponentState(int modifiersCapacity, int multipliersCapacity, int requiredPartsCapacity)
    {
        Modifiers = new(modifiersCapacity);
        Multipliers = new(multipliersCapacity);
        RequiredConsciousnessParts = new(requiredPartsCapacity);
    }

    public bool ForceDead;
    public bool ForceUnconscious;
    public bool IsConscious;
}

[ByRefEvent]
public record struct ConsciousnessUpdatedEvent(bool IsConscious);

[Serializable, DataRecord]
public partial record struct ConsciousnessModifier(FixedPoint2 Change, TimeSpan? Time, ConsciousnessModType Type = ConsciousnessModType.Generic);

[Serializable, DataRecord]
public partial record struct ConsciousnessMultiplier(FixedPoint2 Change, TimeSpan? Time, ConsciousnessModType Type = ConsciousnessModType.Generic);
