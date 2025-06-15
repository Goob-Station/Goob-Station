// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Whitelist;

namespace Content.Goobstation.Shared.Storage;

/// <summary>
/// Simulates contents of the owner's StorageComponent interacting with whitelisted entity.
/// </summary>
[RegisterComponent]
public sealed partial class RelayInteractionStorageComponent : Component
{
    [DataField]
    public EntityWhitelist? Whitelist;
}
