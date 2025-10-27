// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Access.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared.Access.Components;

/// <summary>
/// Toggles an access provider with <c>ItemToggle</c>.
/// Requires <see cref="AccessComponent"/>.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(AccessToggleSystem))]
public sealed partial class AccessToggleComponent : Component;