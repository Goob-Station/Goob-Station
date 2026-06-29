using System.Threading;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Alert;
using Content.Shared.Mood;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Generic;

namespace Content.Pirate.Server.Mood;

[RegisterComponent]
public sealed partial class MoodComponent : Component
{
    [DataField]
    public float CurrentMoodLevel;

    [DataField]
    public MoodThreshold CurrentMoodThreshold;

    [DataField]
    public MoodThreshold LastThreshold;

    [ViewVariables(VVAccess.ReadOnly)]
    public readonly Dictionary<string, string> CategorisedEffects = new();

    [ViewVariables(VVAccess.ReadOnly)]
    public readonly Dictionary<string, float> CategorisedEffectValues = new();

    [ViewVariables(VVAccess.ReadOnly)]
    public readonly Dictionary<string, float> UncategorisedEffects = new();

    [DataField]
    public float SpeedBonusGrowth = 1.003f;

    [DataField]
    public float MinimumSpeedModifier = 0.75f;

    [DataField]
    public float MaximumSpeedModifier = 1.15f;

    [DataField]
    public float IncreaseCritThreshold = 1.2f;

    [DataField]
    public float DecreaseCritThreshold = 0.9f;

    [ViewVariables(VVAccess.ReadOnly)]
    public FixedPoint2 CritThresholdBeforeModify;

    [DataField]
    public ProtoId<AlertCategoryPrototype> MoodCategory = "Mood";

    [DataField(customTypeSerializer: typeof(DictionarySerializer<MoodThreshold, float>))]
    public Dictionary<MoodThreshold, float> MoodThresholds = new()
    {
        { MoodThreshold.Perfect, 100f },
        { MoodThreshold.Exceptional, 80f },
        { MoodThreshold.Great, 70f },
        { MoodThreshold.Good, 60f },
        { MoodThreshold.Neutral, 50f },
        { MoodThreshold.Meh, 40f },
        { MoodThreshold.Bad, 30f },
        { MoodThreshold.Terrible, 20f },
        { MoodThreshold.Horrible, 10f },
        { MoodThreshold.Insane, 5f },
        { MoodThreshold.Dead, 0f },
    };

    [DataField(customTypeSerializer: typeof(DictionarySerializer<MoodThreshold, ProtoId<AlertPrototype>>))]
    public Dictionary<MoodThreshold, ProtoId<AlertPrototype>> MoodThresholdsAlerts = new()
    {
        { MoodThreshold.Dead, "MoodDead" },
        { MoodThreshold.Horrible, "Horrible" },
        { MoodThreshold.Terrible, "Terrible" },
        { MoodThreshold.Bad, "Bad" },
        { MoodThreshold.Meh, "Meh" },
        { MoodThreshold.Neutral, "Neutral" },
        { MoodThreshold.Good, "Good" },
        { MoodThreshold.Great, "Great" },
        { MoodThreshold.Exceptional, "Exceptional" },
        { MoodThreshold.Perfect, "Perfect" },
        { MoodThreshold.Insane, "Insane" },
    };

    [DataField(customTypeSerializer: typeof(DictionarySerializer<string, float>))]
    public Dictionary<string, float> HealthMoodEffectsThresholds = new()
    {
        { "HealthHeavyDamage", 0.8f },
        { "HealthSevereDamage", 0.5f },
        { "HealthLightDamage", 0.1f },
        { "HealthNoDamage", 0.05f },
    };

    // Pirate: TheDen-compatible timeout cancellation for refreshed timed moodlets.
    public Dictionary<ProtoId<MoodEffectPrototype>, CancellationTokenSource> EffectTimeoutSources = new();
}

[Serializable]
public enum MoodThreshold : ushort
{
    Insane = 1,
    Horrible = 2,
    Terrible = 3,
    Bad = 4,
    Meh = 5,
    Neutral = 6,
    Good = 7,
    Great = 8,
    Exceptional = 9,
    Perfect = 10,
    Dead = 0,
}
