// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Roles;

namespace Content.Goobstation.Server.Spy.Roles;

/// <summary>
/// Added to mind role entities to tag that they are a syndicate spy
/// </summary>
[RegisterComponent]
public sealed partial class SpyRoleComponent : BaseMindRoleComponent;
