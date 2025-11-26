// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Roles;

namespace Content.Goobstation.Server.SlaughterDemon.Roles;

/// <summary>
///  Added to mind role entities to tag that they are a slaughter demon.
/// </summary>
[RegisterComponent, Access(typeof(SlaughterDemonSystem))]
public sealed partial class SlaughterDemonRoleComponent : BaseMindRoleComponent;
