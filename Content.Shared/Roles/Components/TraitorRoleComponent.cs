// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.GameStates;

namespace Content.Shared.Roles.Components;

/// <summary>
/// Added to mind role entities to tag that they are a syndicate traitor.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class TraitorRoleComponent : BaseMindRoleComponent;
