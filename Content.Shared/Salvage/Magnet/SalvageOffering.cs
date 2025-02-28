namespace Content.Shared.Salvage.Magnet;

/// <summary>
/// Asteroid offered for the magnet.
/// </summary>
public record struct SalvageOffering : ISalvageMagnetOffering
{
    public SalvageMapPrototype SalvageMap;

    uint ISalvageMagnetOffering.Cost => 1000; // DeltaV: Station gets next to no benefit from you pulling wrecks, force you to mine first.
}
