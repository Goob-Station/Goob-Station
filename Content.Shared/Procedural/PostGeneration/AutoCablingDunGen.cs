// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Procedural.PostGeneration;

/// <summary>
/// Runs cables throughout the dungeon.
/// </summary>
/// <remarks>
/// DungeonData keys are:
/// - Cabling
/// </remarks>
public sealed partial class AutoCablingDunGen : IDunGenLayer;