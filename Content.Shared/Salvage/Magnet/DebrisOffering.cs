// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
namespace Content.Shared.Salvage.Magnet;

/// <summary>
/// Space debis offered for the magnet.
/// </summary>
public record struct DebrisOffering : ISalvageMagnetOffering
{
    public string Id;

    uint ISalvageMagnetOffering.Cost => 0; // DeltaV: Debris is a very good source of materials for the station, so no cost
}