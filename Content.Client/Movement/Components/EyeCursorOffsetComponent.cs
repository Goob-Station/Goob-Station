// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Client.Movement.Systems;
using Content.Shared.Movement.Components;

namespace Content.Client.Movement.Components;

[RegisterComponent]
public sealed partial class EyeCursorOffsetComponent : SharedEyeCursorOffsetComponent
{
    /// <summary>
    /// The location the offset will attempt to pan towards; based on the cursor's position in the game window.
    /// </summary>
    public Vector2 TargetPosition = Vector2.Zero;

    /// <summary>
    /// The current positional offset being applied. Used to enable gradual panning.
    /// </summary>
    public Vector2 CurrentPosition = Vector2.Zero;
}