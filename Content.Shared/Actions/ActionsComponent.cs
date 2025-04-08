// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Actions;

[NetworkedComponent]
[RegisterComponent]
[Access(typeof(SharedActionsSystem))]
public sealed partial class ActionsComponent : Component
{
    /// <summary>
    /// List of actions currently granted to this entity.
    /// On the client, this may contain a mixture of client-side and networked entities.
    /// </summary>
    [DataField] public HashSet<EntityUid> Actions = new();
}

[Serializable, NetSerializable]
public sealed class ActionsComponentState : ComponentState
{
    public readonly HashSet<NetEntity> Actions;

    public ActionsComponentState(HashSet<NetEntity> actions)
    {
        Actions = actions;
    }
}

/// <summary>
///     Determines how the action icon appears in the hotbar for item actions.
/// </summary>
public enum ItemActionIconStyle : byte
{
    /// <summary>
    /// The default - The item icon will be big with a small action icon in the corner
    /// </summary>
    BigItem,

    /// <summary>
    /// The action icon will be big with a small item icon in the corner
    /// </summary>
    BigAction,

    /// <summary>
    /// BigAction but no item icon will be shown in the corner.
    /// </summary>
    NoItem
}