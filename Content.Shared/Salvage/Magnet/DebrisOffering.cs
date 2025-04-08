namespace Content.Shared.Salvage.Magnet;

/// <summary>
/// Space debis offered for the magnet.
/// </summary>
public record struct DebrisOffering : ISalvageMagnetOffering
{
    public string Id;

    uint ISalvageMagnetOffering.Cost => 0; // DeltaV: Debris is a very good source of materials for the station, so no cost
}
