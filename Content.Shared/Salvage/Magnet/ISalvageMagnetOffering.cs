// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
namespace Content.Shared.Salvage.Magnet;

public interface ISalvageMagnetOffering
{
    /// <summary>
    /// DeltaV: How many mining points this offering costs to accept.
    /// </summary>
    public uint Cost { get; }
}