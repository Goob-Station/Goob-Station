// SPDX-FileCopyrightText: 2022 EmoGarbage404 <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Atmos.Components;

/// <summary>
/// Component that can be used to add (or remove) fire stacks when used as a melee weapon.
/// </summary>
[RegisterComponent]
public sealed partial class IgniteOnMeleeHitComponent : Component
{
    [DataField]
    public float FireStacks { get; set; }
}