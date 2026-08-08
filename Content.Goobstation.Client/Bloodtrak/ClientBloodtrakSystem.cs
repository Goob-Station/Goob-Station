// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Bloodtrak;
using Content.Shared.Pinpointer;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;

namespace Content.Goobstation.Client.Bloodtrak;

public sealed class ClientBloodtrakSystem : SharedBloodtrakSystem
{
    [Dependency] private readonly IEyeManager _eyeManager = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<BloodtrakComponent, SpriteComponent>();
        while (query.MoveNext(out var uid, out var pinpointer, out var sprite))
        {
            if (!pinpointer.HasTarget)
                continue;

            var eye = _eyeManager.CurrentEye;
            var angle = pinpointer.ArrowAngle + eye.Rotation;

            switch (pinpointer.DistanceToTarget)
            {
                case Shared.Bloodtrak.Distance.Close:
                case Shared.Bloodtrak.Distance.Medium:
                case Shared.Bloodtrak.Distance.Far:
                    _sprite.LayerSetRotation((uid, sprite), PinpointerLayers.Screen, angle);
                    break;
                default:
                    _sprite.LayerSetRotation((uid, sprite), PinpointerLayers.Screen, Angle.Zero);
                    break;
            }
        }
    }
}
