// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SX_7 <sn1.test.preria.2002@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Whitelist;

namespace Content.Server.Procedural;

/// <summary>
/// Marker that indicates the specified room prototype should occupy this space.
/// </summary>
[RegisterComponent]
public sealed partial class RoomFillComponent : Component
{
    /// <summary>
    /// Are we allowed to rotate room templates?
    /// If the room is not a square this will only do 180 degree rotations.
    /// </summary>
    [DataField]
    public bool Rotation = true;

    /// <summary>
    /// Min size of the possible selected room.
    /// </summary>
    [DataField]
    public Vector2i MinSize = new (3, 3);

    /// <summary>
    /// Max size of the possible selected room.
    /// </summary>
    [DataField]
    public Vector2i MaxSize = new (10, 10);

    /// <summary>
    /// Rooms allowed for the marker.
    /// </summary>
    [DataField]
    public EntityWhitelist? RoomWhitelist;

    /// <summary>
    /// Should any existing entities / decals be bulldozed first.
    /// </summary>
    [DataField]
    public bool ClearExisting = true;
}