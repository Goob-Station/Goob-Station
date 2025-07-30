using Content.Shared.Atmos;
using Robust.Shared.Serialization;

namespace Content.Pirate.Shared.Botany.PlantAnalyzer;

[Serializable, NetSerializable]
public sealed class PlantAnalyzerScannedUserMessage(NetEntity? targetEntity, bool? scanMode, PlantAnalyzerPlantData? plantData, PlantAnalyzerTrayData? trayData, PlantAnalyzerTolerancesData? tolerancesData, PlantAnalyzerProduceData? produceData, TimeSpan? printReadyAt) : BoundUserInterfaceMessage
{
    public readonly NetEntity? TargetEntity = targetEntity;
    public readonly bool? ScanMode = scanMode;
    public readonly PlantAnalyzerPlantData? PlantData = plantData;
    public readonly PlantAnalyzerTrayData? TrayData = trayData;
    public readonly PlantAnalyzerTolerancesData? TolerancesData = tolerancesData;
    public readonly PlantAnalyzerProduceData? ProduceData = produceData;
    public readonly TimeSpan? PrintReadyAt = printReadyAt;
}

/// <summary>
/// Everything that is kept independed of a given plant/seed.
/// </summary>
[Serializable, NetSerializable]
public sealed class PlantAnalyzerTrayData(float waterLevel, float nutritionLevel, float toxins, float pestLevel, float weedLevel, List<string>? chemicals)
{
    public float WaterLevel = waterLevel;
    public float NutritionLevel = nutritionLevel;
    public float Toxins = toxins;
    public float PestLevel = pestLevel;
    public float WeedLevel = weedLevel;
    public List<string>? Chemicals = chemicals;
}


/// <summary>
/// All the information to keep the plant alive.
/// Which is most of the "Tolerances" reagion plus the gases it may need.
/// </summary>
[Serializable, NetSerializable]
public sealed class PlantAnalyzerTolerancesData(float nutrientConsumption, float waterConsumption, float idealHeat, float heatTolerance, float idealLight, float lightTolerance, float toxinsTolerance, float lowPressureTolerance, float highPressureTolerance, float pestTolerance, float weedTolerance, List<Gas> consumeGasses)
{
    public float WaterConsumption = waterConsumption;
    public float NutrientConsumption = nutrientConsumption;
    public float ToxinsTolerance = toxinsTolerance;
    public float PestTolerance = pestTolerance;
    public float WeedTolerance = weedTolerance;
    public float IdealPressure = (lowPressureTolerance + highPressureTolerance) / 2;
    public float PressureTolerance = (lowPressureTolerance + highPressureTolerance) / 2 - lowPressureTolerance;
    public float IdealHeat = idealHeat;
    public float HeatTolerance = heatTolerance;
    public float IdealLight = idealLight;
    public float LightTolerance = lightTolerance;
    public List<Gas> ConsumeGasses = consumeGasses;
}

/// <summary>
/// Information about the plant inside the tray.
/// </summary>
[Serializable, NetSerializable]
public sealed class PlantAnalyzerPlantData(string seedDisplayName, float health, float endurance, float age, float lifespan, bool dead, bool viable, bool mutating, bool kudzu)
{
    public string SeedDisplayName = seedDisplayName;
    public float Health = health;
    public float Endurance = endurance;
    public float Age = age;
    public float Lifespan = lifespan;
    public bool Dead = dead;
    public bool Viable = viable;
    public bool Mutating = mutating;
    public bool Kudzu = kudzu;
}

/// <summary>
/// Information about the output of a plant (produce and gas).
/// </summary>
[Serializable, NetSerializable]
public sealed class PlantAnalyzerProduceData(int yield, float potency, List<string> chemicals, List<string> produce, List<Gas> exudeGasses, bool seedless)
{
    public int Yield = yield;
    public string Potency = ObscurePotency(potency);
    public List<string> Chemicals = chemicals;
    public List<string> Produce = produce;
    public List<Gas> ExudeGasses = exudeGasses;
    public bool Seedless = seedless;

    private static readonly Dictionary<float, string> PotencyThresholds = new()
    {
        { 5f, "tiny" },
        { 10f, "small" },
        { 15f, "below-average" },
        { 20f, "average" },
        { 25f, "above-average" },
        { 30f, "large" },
        { 40f, "huge" },
        { 50f, "gigantic" },
        { 60f, "ludicrous" }
    };

    private static string ObscurePotency(float potency)
    {
        foreach (var threshold in PotencyThresholds)
        {
            if (potency <= threshold.Key)
            {
                return "plant-analyzer-potency-" + threshold.Value;
            }
        }

        return "plant-analyzer-potency-immeasurable";
    }
}

[Serializable, NetSerializable]
public sealed class PlantAnalyzerPrintMessage : BoundUserInterfaceMessage
{
}
