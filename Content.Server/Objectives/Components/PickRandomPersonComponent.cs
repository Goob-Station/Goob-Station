// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Marcus F <199992874+thebiggestbruh@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SX_7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Objectives.Systems;
using Content.Shared.Mind.Filters;

namespace Content.Server.Objectives.Components;

/// <summary>
/// Sets the target for <see cref="TargetObjectiveComponent"/> to a random person from a pool and filters.
/// </summary>
/// <remarks>
/// Don't copy paste this for a new objective, if you need a new filter just make a new filter and set it in YAML.
/// </remarks>
[RegisterComponent, Access(typeof(PickObjectiveTargetSystem))]
public sealed partial class PickRandomPersonComponent : Component
{
    /// <summary>
    /// A pool to pick potential targets from.
    /// </summary>
    [DataField]
    public IMindPool Pool = new AliveHumansPool();

    /// <summary>
    /// Filters to apply to <see cref="Pool"/>.
    /// </summary>
    [DataField]
    public List<MindFilter> Filters = new();
}
