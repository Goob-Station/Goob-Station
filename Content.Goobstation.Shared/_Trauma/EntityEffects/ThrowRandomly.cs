// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Content.Shared.Throwing;
using Content.Goobstation.Shared._Trauma.EntityEffects.Throw;
using Robust.Shared.Random;

namespace Content.Goobstation.Shared._Trauma.EntityEffects;

/// <summary>
/// Throws the target entity in a random direction, with a fixed speed.
/// </summary>
public sealed partial class ThrowRandomly : BaseThrowEntityEffect<ThrowRandomly>;

public sealed partial class ThrowRandomlyEffectSystem : EntityEffectSystem<MetaDataComponent, ThrowRandomly>
{
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private ThrowingSystem _throwing = default!;

    protected override void Effect(Entity<MetaDataComponent> ent, ref EntityEffectEvent<ThrowRandomly> args)
    {
        var angle = _random.NextAngle();
        var direction = angle.ToVec();

        var effect = args.Effect;
        _throwing.TryThrow(ent,
            direction,
            baseThrowSpeed: effect.Speed,
            user: args.User);
    }
}
