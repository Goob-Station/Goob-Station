// SPDX-FileCopyrightText: 2022 Javier Guardia Fernández <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
//
// SPDX-License-Identifier: MIT
using Robust.Client.GameObjects;
using Robust.Shared.GameObjects;

namespace Content.MapRenderer.Painters;

public readonly record struct EntityData(EntityUid Owner, SpriteComponent Sprite, float X, float Y)
{
    public readonly EntityUid Owner = Owner;

    public readonly SpriteComponent Sprite = Sprite;

    public readonly float X = X;

    public readonly float Y = Y;
}