// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SX_7 <sn1.test.preria.2002@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;

namespace Content.Client.Animations;

/// <summary>
/// Entities with this component tracks the user's world position every frame.
/// </summary>
[RegisterComponent]
public sealed partial class TrackUserComponent : Component
{
    public EntityUid? User;

    /// <summary>
    /// Offset in the direction of the entity's rotation.
    /// </summary>
    public Vector2 Offset = Vector2.Zero;
}