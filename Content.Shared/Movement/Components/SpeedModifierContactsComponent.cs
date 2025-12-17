// SPDX-FileCopyrightText: 2021 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Movement.Systems;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Shared.Movement.Components;

/// <summary>
/// Component that modifies the movement speed of other entities that come into contact with the entity this component is added to.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(SpeedModifierContactsSystem))]
public sealed partial class SpeedModifierContactsComponent : Component
{
    /// <summary>
    /// The modifier applied to the walk speed of entities that come into contact with the entity this component is added to.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float WalkSpeedModifier = 1.0f;

    /// <summary>
    /// The modifier applied to the sprint speed of entities that come into contact with the entity this component is added to.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float SprintSpeedModifier = 1.0f;

    /// <summary>
    /// Indicates whether this component affects the movement speed of airborne entities that come into contact with the entity this component is added to.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool AffectAirborne;

    /// <summary>
    /// A whitelist of entities that should be ignored by this component's speed modifiers.
    /// </summary>
    [DataField]
    public EntityWhitelist? IgnoreWhitelist;

    // Goobstation
    [DataField]
    public EntityWhitelist? Whitelist;
}
