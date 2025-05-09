// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 RatherUncreative <RatherUncreativeName@proton.me>
// SPDX-FileCopyrightText: 2025 Ted Lukin <66275205+pheenty@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Whatstone <whatston3@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._NF.Interaction.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared._NF.Interaction.Components;

/// <summary>
/// When an entity with this is removed from a hand, it is replaced with an existing placeholder entity.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(HandPlaceholderSystem))]
[AutoGenerateComponentState]
public sealed partial class HandPlaceholderRemoveableComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid Placeholder;

    /// <summary>
    /// Used to prevent it incorrectly replacing with the placeholder,
    /// when selecting and deselecting a module.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Enabled;
}