using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Hydroponics.Mutations.MassMushroomOrganism;

[RegisterComponent, NetworkedComponent]
public sealed partial class FungalInfectionComponent : Component
{
    [DataField]
    public EntityUid AttachedMushroomOrganism;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextUpdateTime = TimeSpan.Zero;

    [DataField]
    public TimeSpan TimeToNewPhase = TimeSpan.FromSeconds(150);

    [DataField]
    public TimeSpan UpdateInterval = TimeSpan.Zero;

    [DataField]
    public FungalInfectionPhase CurrentPhase = FungalInfectionPhase.First;

    [DataField]
    public FungalInfectionThreshold CurrentThreshold = FungalInfectionThreshold.None;

    [DataField]
    public FungalInfectionThreshold LastThreshold = FungalInfectionThreshold.None;

    [DataField]
    public float PoisonDamage = 0.3f;

    [DataField]
    public float PoisonDamageMultiply = 3f;

    [DataField]
    public float HungerModifyValue = 2f;

    [DataField]
    public float ThirstModifyValue = 2f;

    [DataField]
    public float DrunkennessValue = 100f;

    [DataField]
    public float WalkSpeedValueFirstPhase = 2f;

    [DataField]
    public float SprintSpeedValueFirstPhase = 3.5f;

    [DataField]
    public float WalkSpeedValueSecondPhase = 1f;

    [DataField]
    public float SprintSpeedValueSecondPhase = 1.5f;

    [DataField]
    public Dictionary<string, float> MushroomPrototypes = new()
    {
        { "MassMushroomOrganismPolymorph", 10f },
        { "MassMushroomOrganismAltPolymorph", 2f },
    };

    [DataField]
    public bool IsPoisonDamageInitialized;
    [DataField]
    public bool IsPoisonDamageMultiplyed;
    [DataField]
    public bool IsHungerThirstModifyInitialized;

    [DataField]
    public Dictionary<FungalInfectionThreshold, float> InfectionThresholds = new()
    {
        { FungalInfectionThreshold.First, 20.0f },
        { FungalInfectionThreshold.Second, 50.0f },
        { FungalInfectionThreshold.Third, 80.0f },
        { FungalInfectionThreshold.Fourth, 100.0f },
    };

    [DataField]
    public List<FungalInfectionPhase> InfectionPhases = new()
    {
        FungalInfectionPhase.First,
        FungalInfectionPhase.Second,
    };

    [DataField]
    public float InfectionProgress;

    [DataField]
    public float InfectionProgressBonus = 1.0f;

    [DataField]
    public string FirstStageVisual = "stage-1";
    [DataField]
    public string SecondStageVisual = "stage-2";
}

public sealed partial class FungalGrowthEvent : InstantActionEvent;
public sealed partial class SporeDivisionEvent : InstantActionEvent;

public enum FungalInfectionThreshold : byte
{
    None,
    First,
    Second,
    Third,
    Fourth,
}

[Serializable, NetSerializable]
public enum FungalInfectionPhase : byte
{
    None,
    First,
    Second,
}

[Serializable, NetSerializable]
public enum FungalInfectionPhaseVisualsStage : byte
{
    InfectionStage,
}

[Serializable, NetSerializable]
public enum FungalInfectionPhaseVisuals : byte
{
    First,
    Second
}

[Serializable, NetSerializable]
public sealed partial class SporeDivisionDoAfterEvent : SimpleDoAfterEvent { }
