// SPDX-FileCopyrightText: 2023 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Roles;

namespace Content.Server.Roles;

/// <summary>
///     Added to mind role entities to tag that they are a thief.
/// </summary>
[RegisterComponent]
public sealed partial class ThiefRoleComponent : BaseMindRoleComponent
{
}