// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Containers;
using Robust.Shared.GameStates;

namespace Content.Shared.Actions;

/// <summary>
/// This component indicates that this entity contains actions inside of some container.
/// </summary>
[NetworkedComponent, RegisterComponent]
[Access(typeof(ActionContainerSystem), typeof(SharedActionsSystem))]
public sealed partial class ActionsContainerComponent : Component
{
    public const string ContainerId = "actions";

    [ViewVariables]
    public Container Container = default!;
}