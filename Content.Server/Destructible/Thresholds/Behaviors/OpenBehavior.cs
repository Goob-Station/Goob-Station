// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Nutrition.EntitySystems;

namespace Content.Server.Destructible.Thresholds.Behaviors;

/// <summary>
/// Causes the drink/food to open when the destruction threshold is reached.
/// If it is already open nothing happens.
/// </summary>
[DataDefinition]
public sealed partial class OpenBehavior : IThresholdBehavior
{
    public void Execute(EntityUid uid, DestructibleSystem system, EntityUid? cause = null)
    {
        var openable = system.EntityManager.System<OpenableSystem>();
        openable.TryOpen(uid);
    }
}