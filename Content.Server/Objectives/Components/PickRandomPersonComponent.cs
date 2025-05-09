// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Objectives.Systems;

namespace Content.Server.Objectives.Components;

/// <summary>
/// Sets the target for <see cref="TargetObjectiveComponent"/> to a random person.
/// </summary>
[RegisterComponent]
public sealed partial class PickRandomPersonComponent : Component
{
    [DataField]
    public bool NeedsOrganic; // Goobstation: Only pick non-silicon players.
}