// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Hannah Giovanna Dawson <karakkaraz@gmail.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Xenoarchaeology.Equipment.Components;

/// <summary>
/// This is used for a machine that biases
/// an artifact placed on it to move up/down
/// </summary>
[RegisterComponent]
public sealed partial class TraversalDistorterComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public BiasDirection BiasDirection = BiasDirection.Up;

    public TimeSpan NextActivation = default!;
    public TimeSpan ActivationDelay = TimeSpan.FromSeconds(1);
}

public enum BiasDirection : byte
{
    Up, //Towards depth 0
    Down, //Away from depth 0
}