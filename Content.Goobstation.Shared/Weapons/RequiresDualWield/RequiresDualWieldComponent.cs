// SPDX-FileCopyrightText: 2024 ScyronX <166930367+ScyronX@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Weapons.RequiresDualWield;

/// <summary>
/// Makes a weapon only able to be shot while dual wielding.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(RequiresDualWieldSystem))]
public sealed partial class RequiresDualWieldComponent : Component
{
    public TimeSpan LastPopup;

    [DataField]
    public TimeSpan PopupCooldown = TimeSpan.FromSeconds(1);

    [DataField]
    public LocId? WieldRequiresExamineMessage  = "gun-requires-dual-wield-component-examine";

    [DataField]
    public EntityWhitelist? Whitelist;
}