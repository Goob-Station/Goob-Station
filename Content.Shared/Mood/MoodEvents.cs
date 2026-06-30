using Content.Shared.Alert;
using Robust.Shared.Serialization;

namespace Content.Shared.Mood;

[Serializable, NetSerializable]
public sealed class MoodEffectEvent : EntityEventArgs
{
    public string EffectId;
    public float EffectModifier = 1f;
    public float EffectOffset;

    public MoodEffectEvent(string effectId, float effectModifier = 1f, float effectOffset = 0f)
    {
        EffectId = effectId;
        EffectModifier = effectModifier;
        EffectOffset = effectOffset;
    }
}

[Serializable, NetSerializable]
public sealed class MoodRemoveEffectEvent : EntityEventArgs
{
    public string EffectId;

    public MoodRemoveEffectEvent(string effectId)
    {
        EffectId = effectId;
    }
}

/// <summary>
/// Raised when final mood is calculated, allowing other systems to alter the result.
/// </summary>
[ByRefEvent]
public record struct OnSetMoodEvent(EntityUid Receiver, float MoodChangedAmount, bool Cancelled, float MoodOffset = 0f);

/// <summary>
/// Raised when a moodlet is received, before that moodlet is applied.
/// </summary>
[ByRefEvent]
public record struct OnMoodEffect(EntityUid Receiver, string EffectId, float EffectModifier = 1, float EffectOffset = 0);

public sealed partial class ShowMoodAlertEvent : BaseAlertEvent;
