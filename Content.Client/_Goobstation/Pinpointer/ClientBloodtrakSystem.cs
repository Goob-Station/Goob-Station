using Content.Shared._Goobstation.Pinpointer;
using Content.Shared.Pinpointer;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;

namespace Content.Client._Goobstation.Pinpointer;

public sealed class ClientBloodtrakSystem : SharedBloodtrakSystem
{
    [Dependency] private readonly IEyeManager _eyeManager = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // we want to show pinpointers arrow direction relative
        // to players eye rotation (like it was in SS13)

        // because eye can change it rotation anytime
        // we need to update this arrow in a update loop
        var query = EntityQueryEnumerator<BloodtrakComponent, SpriteComponent>();
        while (query.MoveNext(out var _, out var pinpointer, out var sprite))
        {
            if (!pinpointer.HasTarget)
                continue;
            var eye = _eyeManager.CurrentEye;
            var angle = pinpointer.ArrowAngle + eye.Rotation;

            switch (pinpointer.DistanceToTarget)
            {
                case Shared._Goobstation.Pinpointer.Distance.Close:
                case Shared._Goobstation.Pinpointer.Distance.Medium:
                case Shared._Goobstation.Pinpointer.Distance.Far:
                    sprite.LayerSetRotation(PinpointerLayers.Screen, angle);
                    break;
                default:
                    sprite.LayerSetRotation(PinpointerLayers.Screen, Angle.Zero);
                    break;
            }
        }
    }
}
