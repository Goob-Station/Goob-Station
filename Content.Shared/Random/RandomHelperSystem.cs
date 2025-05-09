// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Robust.Shared.Random;

namespace Content.Shared.Random;

/// <summary>
///     System containing various content-related random helpers.
/// </summary>
public sealed class RandomHelperSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public void RandomOffset(EntityUid entity, float minX, float maxX, float minY, float maxY)
    {
        var randomX = _random.NextFloat() * (maxX - minX) + minX;
        var randomY = _random.NextFloat() * (maxY - minY) + minY;
        var offset = new Vector2(randomX, randomY);

        var xform = Transform(entity);
        _transform.SetLocalPosition(xform, xform.LocalPosition + offset);
    }

    public void RandomOffset(EntityUid entity, float min, float max)
    {
        RandomOffset(entity, min, max, min, max);
    }

    public void RandomOffset(EntityUid entity, float value)
    {
        RandomOffset(entity, -value, value);
    }
}