// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Content.Server.Damage.Systems;

namespace Content.Server.Damage.Components;

[RegisterComponent, Access(typeof(DamageRandomPopupSystem))]
/// <summary>
/// Outputs a random pop-up from the list when an object receives damage
/// </summary>
public sealed partial class DamageRandomPopupComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public List<LocId> Popups = new();
}
