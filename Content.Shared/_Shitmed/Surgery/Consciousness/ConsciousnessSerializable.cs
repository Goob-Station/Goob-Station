﻿using Content.Shared._Shitmed.Surgery.Pain;
using Content.Shared.FixedPoint;
using Robust.Shared.Serialization;

namespace Content.Shared._Shitmed.Surgery.Consciousness;

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

    public readonly Dictionary<(NetEntity, string), ConsciousnessModifier> Modifiers = new();
    public readonly Dictionary<(NetEntity, string), ConsciousnessMultiplier> Multipliers = new();
    public readonly Dictionary<string, (NetEntity?, bool, bool)> RequiredConsciousnessParts = new();

    public bool ForceDead;
    public bool ForceUnconscious;
    public bool IsConscious;
}

[ByRefEvent]
public record struct ConsciousnessUpdatedEvent(bool IsConscious, FixedPoint2 ConsciousnessDelta);

[Serializable, DataRecord]
public record struct ConsciousnessModifier(FixedPoint2 Change, ConsciousnessModType Type = ConsciousnessModType.Generic);

[Serializable, DataRecord]
public record struct ConsciousnessMultiplier(FixedPoint2 Change, ConsciousnessModType Type = ConsciousnessModType.Generic);
