// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared.Effects;

/// <summary>
/// Raised on the server and sent to a client to play the color flash animation.
/// </summary>
[Serializable, NetSerializable]
public sealed class ColorFlashEffectEvent : EntityEventArgs
{
    /// <summary>
    /// Color to play for the flash.
    /// </summary>
    public Color Color;

    public List<NetEntity> Entities;

    public ColorFlashEffectEvent(Color color, List<NetEntity> entities)
    {
        Color = color;
        Entities = entities;
    }
}