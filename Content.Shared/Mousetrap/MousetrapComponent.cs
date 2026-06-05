// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 potato1234_x <79580518+potato1234x@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Magnus Larsen <i.am.larsenml@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Mousetrap;

/// <summary>
/// Component inteded to be used for mouse traps.
/// Will stop step triggers from happening unless armed via <see cref="Item.ItemToggle.Components.ItemToggleComponent"/>
/// and will scale damage taken from <see cref="Trigger.Components.Effects.DamageOnTriggerComponent"/>
/// depending on mass.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MousetrapComponent : Component
{
    /// <summary>
    /// Set this to change where the
    /// inflection point in the damage scaling
    /// equation will occur.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int MassBalance = 10;
}