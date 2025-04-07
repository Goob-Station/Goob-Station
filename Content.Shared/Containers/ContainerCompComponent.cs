// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Containers;

/// <summary>
/// Applies container changes whenever an entity is inserted into the specified container on this entity.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ContainerCompComponent : Component
{
    [DataField(required: true)]
    public EntProtoId Proto;

    [DataField(required: true)]
    public string Container = string.Empty;
}