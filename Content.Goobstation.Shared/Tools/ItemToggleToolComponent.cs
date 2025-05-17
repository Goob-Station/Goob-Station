// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Tools;

/// <summary>
/// Makes a tool with <c>ItemToggle</c> require being on to be used for its tool qualities.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ItemToggleToolComponent : Component;
