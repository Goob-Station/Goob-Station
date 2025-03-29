using Content.Goobstation.Shared.Bloodtrak;
using Content.Shared.Pinpointer;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;

namespace Content.Goobstation.Client.Bloodtrak;

public sealed class ClientBloodtrakSystem : SharedBloodtrakSystem
{
    [Dependency] private readonly IEyeManager _eyeManager = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<BloodtrakComponent, SpriteComponent>();
        while (query.MoveNext(out var uid, out var pinpointer, out var sprite))
        {
            if (!pinpointer.HasTarget)
                continue;

            var eye = _eyeManager.CurrentEye;

            // Convert server-provided world angle to eye-relative space
            var angle = (pinpointer.ArrowAngle - eye.Rotation).FlipPositive();

            // Update sprite rotation only if needed
            if (pinpointer.DistanceToTarget is Shared.Bloodtrak.Distance.Close or Shared.Bloodtrak.Distance.Medium or Shared.Bloodtrak.Distance.Far)
            {
                sprite.LayerSetRotation(PinpointerLayers.Screen, angle);
                sprite.LayerSetVisible(PinpointerLayers.Screen, true);
            }
            else
            {
                sprite.LayerSetVisible(PinpointerLayers.Screen, false);
            }
        }
    }
}
