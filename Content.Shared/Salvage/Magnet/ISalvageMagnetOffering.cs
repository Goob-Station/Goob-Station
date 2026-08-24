// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Salvage.Magnet;

public interface ISalvageMagnetOffering
{
    /// <summary>
    /// DeltaV: How many mining points this offering costs to accept.
    /// </summary>
    public uint Cost { get; }
}