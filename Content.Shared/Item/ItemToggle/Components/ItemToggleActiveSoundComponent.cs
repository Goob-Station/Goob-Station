// SPDX-FileCopyrightText: 2023 Darkie <darksaiyanis@gmail.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.Item.ItemToggle.Components;

/// <summary>
/// Handles the active sound being played continuously with some items that are activated (ie e-sword hum).
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ItemToggleActiveSoundComponent : Component
{
    /// <summary>
    ///     The continuous noise this item makes when it's activated (like an e-sword's hum).
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public SoundSpecifier? ActiveSound;

    /// <summary>
    ///     Used when the item emits sound while active.
    /// </summary>
    [DataField]
    public EntityUid? PlayingStream;
}