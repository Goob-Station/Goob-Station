// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Movement.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared.Movement.Components;

/// <summary>
/// Raises the engine movement inputs for a particular entity onto the designated entity
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
[Access(typeof(SharedMoverController))]
public sealed partial class RelayInputMoverComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public EntityUid RelayEntity;
}