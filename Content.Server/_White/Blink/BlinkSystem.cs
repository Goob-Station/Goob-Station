using System.Linq;
using System.Numerics;
using Content.Shared._White.Blink;
using Content.Shared._White.Standing;
using Content.Shared.Physics;
using Content.Shared.Timing;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Physics;

namespace Content.Server._White.Blink;

public sealed class BlinkSystem : SharedBlinkSystem
{
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly PhysicsSystem _physics = default!;
    [Dependency] private readonly TelefragSystem _telefrag = default!;
    [Dependency] private readonly UseDelaySystem _useDelay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<BlinkEvent>(OnBlink);
    }

    private void OnBlink(BlinkEvent msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity == null)
            return;

        var user = args.SenderSession.AttachedEntity.Value;

        if (!TryComp(user, out TransformComponent? xform))
            return;

        var weapon = GetEntity(msg.Weapon);

        if (!TryComp(weapon, out BlinkComponent? blink) || !blink.IsActive ||
            !TryComp(weapon, out UseDelayComponent? delay) || _useDelay.IsDelayed((weapon, delay), blink.BlinkDelay))
            return;

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

        _useDelay.TryResetDelay((weapon, delay), id: blink.BlinkDelay);
        _transform.SetWorldPosition(user, targetPos);
        _telefrag.DoTelefrag(user, xform.Coordinates, blink.KnockdownTime);
        _audio.PlayPvs(blink.BlinkSound, user);
    }
}
