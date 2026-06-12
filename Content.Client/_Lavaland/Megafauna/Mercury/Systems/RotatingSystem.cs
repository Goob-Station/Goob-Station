using Content.Shared._Lavaland.Megafauna.Mercury.Components;
using Robust.Client.GameObjects;
using Robust.Shared.Timing;

namespace Content.Client._Lavaland.Megafauna.Mercury.Systems;

public sealed class RotatingSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<RotatingComponent, SpriteComponent>();
        while (query.MoveNext(out var uid, out var comp, out var sprite))
        {
            if (_timing.CurTime < comp.NextUpdate)
                continue;

            comp.NextUpdate = _timing.CurTime + comp.Interval;

            if (TerminatingOrDeleted(uid))
                continue;

            // If it should increase the rotation speed overtime, do so, with the cap in mind.
            if (comp.IncreaseOvertime)
            {
                comp.CurrentSpeed = Math.Min(comp.CurrentSpeed + comp.IncreaseBy * frameTime, comp.MaximumSpeed);
            }
            else // Just sets the current speed of rotation to the specified set speed at which it should be rotating.
            {
                comp.CurrentSpeed = comp.RotationSpeed;
            }
            // Rotate the sprite by however much it is set to do
            sprite.Rotation += Angle.FromDegrees(comp.CurrentSpeed * frameTime);
        }
    }
}
