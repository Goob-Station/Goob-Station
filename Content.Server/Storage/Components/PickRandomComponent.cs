// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Storage.EntitySystems;
using Content.Shared.Whitelist;

namespace Content.Server.Storage.Components;

/// <summary>
/// Adds a verb to pick a random item from a container.
/// Only picks items that match the whitelist.
/// </summary>
[RegisterComponent]
[Access(typeof(PickRandomSystem))]
public sealed partial class PickRandomComponent : Component
{
    /// <summary>
    /// Whitelist for potential picked items.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public EntityWhitelist? Whitelist;

    /// <summary>
    /// Locale id for the pick verb text.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public LocId VerbText = "comp-pick-random-verb-text";

    /// <summary>
    /// Locale id for the empty storage message.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public LocId EmptyText = "comp-pick-random-empty";
}