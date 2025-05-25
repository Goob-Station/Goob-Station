// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Shitmed.Autodoc.Systems;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitmed.Autodoc.Components;

/// <summary>
/// Creates a list of hands and spawns items to fill them.
/// </summary>
[RegisterComponent, Access(typeof(HandsFillSystem))]
public sealed partial class HandsFillComponent : Component
{
    /// <summary>
    /// The name of each hand and the item to fill it with, if any.
    /// </summary>
    [DataField(required: true)]
    public Dictionary<string, EntProtoId?> Hands = new();
}