// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.Electrocution;

/// <summary>
/// Allow an entity to see the Electrocution HUD showing electrocuted doors.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShowElectrocutionHUDComponent : Component;