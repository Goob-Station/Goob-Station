// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Lathe.Components;

/// <summary>
/// For EntityQuery to keep track of which lathes are producing
/// </summary>
[RegisterComponent]
public sealed partial class LatheProducingComponent : Component
{
    /// <summary>
    /// The time at which production began
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan StartTime;

    /// <summary>
    /// How long it takes to produce the recipe.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan ProductionLength;
}
