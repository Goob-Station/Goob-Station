// SPDX-FileCopyrightText: 2024 Dvir <39403717+dvir001@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Atmos;
using Robust.Shared.Serialization;

namespace Content.Shared._NF.PlantAnalyzer;

/// <summary>
///     The information about the last scanned plant/seed is stored here.
/// </summary>
[Serializable, NetSerializable]
public sealed class PlantAnalyzerScannedSeedPlantInformation : BoundUserInterfaceMessage
{
    public NetEntity? TargetEntity;
    public bool IsTray;

    public string? SeedName;
    public string[]? SeedChem;
    public AnalyzerHarvestType HarvestType;
    public Gas[]? ExudeGases;
    public Gas[]? ConsumeGases;
    public float Endurance;
    public int SeedYield;
    public float Lifespan;
    public float Maturation;
    public float Production;
    public int GrowthStages;
    public float SeedPotency;
    public string[]? Speciation; // Currently only available on server, we need to send strings to the client.
    public AdvancedScanInfo? AdvancedInfo;
}

/// <summary>
///     Information gathered in an advanced scan.
/// </summary>
[Serializable, NetSerializable]
public struct AdvancedScanInfo
{
    public float NutrientConsumption;
    public float WaterConsumption;
    public float IdealHeat;
    public float HeatTolerance;
    public float IdealLight;
    public float LightTolerance;
    public float ToxinsTolerance;
    public float LowPressureTolerance;
    public float HighPressureTolerance;
    public float PestTolerance;
    public float WeedTolerance;
    public MutationFlags Mutations;
}

// Note: currently leaving out Viable.
[Flags]
public enum MutationFlags : byte
{
    None = 0,
    TurnIntoKudzu = 1,
    Seedless = 2,
    Slip = 4,
    Sentient = 8,
    Ligneous = 16,
    Bioluminescent = 32,
    CanScream = 64,
}

public enum AnalyzerHarvestType : byte  // actual HarvestType only exists in server
{
    Repeat,
    NoRepeat,
    SelfHarvest,
}

[Serializable, NetSerializable]
public sealed class PlantAnalyzerSetMode : BoundUserInterfaceMessage
{
    public bool AdvancedScan { get; }
    public PlantAnalyzerSetMode(bool advancedScan)
    {
        AdvancedScan = advancedScan;
    }
}
