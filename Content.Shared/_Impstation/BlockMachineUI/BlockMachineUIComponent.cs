// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 beck <163376292+widgetbeck@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Shared._Impstation.BlockMachineUI;

[RegisterComponent, NetworkedComponent]
public sealed partial class BlockMachineUIComponent : Component
{
    /// <summary>
    /// These entities will be allowed through. If null, only the blacklist applies.
    /// If both are null, blocks all entities.
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist = null;

    /// <summary>
    /// These entities will *not* be allowed through. If null, only the whitelist applies.
    /// If both are null, blocks all entities.
    /// </summary>
    [DataField]
    public EntityWhitelist? Blacklist = null;

    [DataField]
    public LocId? PopupText = "block-machine-ui-cant-use";
}
