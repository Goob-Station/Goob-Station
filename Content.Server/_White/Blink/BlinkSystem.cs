using System.Linq;
using System.Numerics;
using Content.Shared._White.Blink;
using Content.Shared._White.Standing;
using Content.Shared.Physics;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Physics;
using Robust.Shared.Timing;

namespace Content.Server._White.Blink;

public sealed class BlinkSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly PhysicsSystem _physics = default!;
    [Dependency] private readonly SharedLayingDownSystem _layingDown = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeAllEvent<BlinkEvent>(OnBlink);
    }

    private void OnBlink(BlinkEvent msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity == null)
            return;

        var user = args.SenderSession.AttachedEntity.Value;

        if (!TryComp(user, out TransformComponent? xform))
            return;

        if (!TryComp(GetEntity(msg.Weapon), out BlinkComponent? blink))
            return;

        if (blink.NextBlink > _timing.CurTime)
            return;

        var blinkRate = TimeSpan.FromSeconds(1f / blink.BlinkRate);

        blink.NextBlink = _timing.CurTime + blinkRate;

        var coords = _transform.GetWorldPosition(xform);
        var dir = msg.Direction.Normalized();
        var range = MathF.Min(blink.Distance, msg.Direction.Length());

        var ray = new CollisionRay(coords, dir, (int) (CollisionGroup.Impassable | CollisionGroup.InteractImpassable));
        var rayResults = _physics.IntersectRayWithPredicate(xform.MapID, ray, range, x => x == user, false).ToList();

        Vector2 targetPos;
        if (rayResults.Count > 0)
            targetPos = rayResults.MinBy(x => (x.HitPos - coords).Length()).HitPos - dir;
        else
            targetPos = coords + (msg.Direction.Length() > blink.Distance ? dir * blink.Distance : msg.Direction);

        _transform.SetWorldPosition(user, targetPos);
        _layingDown.LieDownInRange(user, xform.Coordinates);
        _audio.PlayPvs(blink.BlinkSound, user);
    }
}
