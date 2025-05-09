// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Map;

namespace Content.Server.Movement.Components;

/// <summary>
/// Added when an entity is being ctrl-click moved when pulled.
/// </summary>
[RegisterComponent]
public sealed partial class PullMovingComponent : Component
{
    // Not serialized to indicate THIS CODE SUCKS, fix pullcontroller first
    [ViewVariables]
    public EntityCoordinates MovingTo;
}