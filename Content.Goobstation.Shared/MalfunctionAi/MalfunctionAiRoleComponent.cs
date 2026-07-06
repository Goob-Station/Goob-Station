// SPDX-FileCopyrightText: 2026 Jonikibaka
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Roles.Components;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.MalfunctionAi;

/// <summary>
/// Added to mind role entities to tag that they are a Malfunction AI antagonist.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MalfunctionAiRoleComponent : BaseMindRoleComponent;
