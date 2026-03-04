// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Storage.EntitySystems;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Shared.Storage.Components;

/// <summary>
/// Adds a verb to pick a random item from a container.
/// Only picks items that match the whitelist.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(PickRandomSystem))]
public sealed partial class PickRandomComponent : Component
{
    /// <summary>
    /// Whitelist for potential picked items.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityWhitelist? Whitelist;

    /// <summary>
    /// Locale id for the pick verb text.
    /// </summary>
    [DataField, AutoNetworkedField]
    public LocId VerbText = "comp-pick-random-verb-text";

    /// <summary>
    /// Locale id for the empty storage message.
    /// </summary>
    [DataField, AutoNetworkedField]
    public LocId EmptyText = "comp-pick-random-empty";
}
