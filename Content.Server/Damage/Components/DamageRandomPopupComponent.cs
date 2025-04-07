// SPDX-FileCopyrightText: 2023 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

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