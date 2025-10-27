// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Electrocution;

/// <summary>
/// Handles toggling sprite layers for the electrocution HUD to show if an entity with the ElectrifiedComponent is electrified.
/// </summary>
[RegisterComponent]
public sealed partial class ElectrocutionHUDVisualsComponent : Component;