// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Movement.Components;

/// <summary>
/// Is this entity always considered to be touching a wall?
/// i.e. when weightless they're floaty but still have free movement.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MovementAlwaysTouchingComponent : Component
{

}