// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Roles;
using Content.Shared.Roles.Components;

namespace Content.Shared._Shitmed.Roles;

/// <summary>
///     Added to mind role entities to tag that they are an Abductor Victim.
/// </summary>
[RegisterComponent]
public sealed partial class AbductorVictimRoleComponent : BaseMindRoleComponent;
