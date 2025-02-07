using System.Linq;
using System.Numerics;
using Content.Shared._White.Standing;
using Content.Shared.Interaction.Events;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Timing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;

namespace Content.Shared._White.Blink;

public abstract class SharedBlinkSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly TelefragSystem _telefrag = default!;
    [Dependency] private readonly UseDelaySystem _useDelay = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlinkComponent, UseInHandEvent>(OnUseInHand);
        SubscribeAllEvent<BlinkEvent>(OnBlink);
    }

    private void OnUseInHand(Entity<BlinkComponent> ent, ref UseInHandEvent args)
    {
        if (args.Handled)
            return;

        ent.Comp.IsActive = !ent.Comp.IsActive;
        var message = ent.Comp.IsActive ? "blink-activated-message" : "blink-deactivated-message";
        _popup.PopupClient(Loc.GetString(message), args.User);
        Dirty(ent);
        args.Handled = true;
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
        var rayResults = _physics.IntersectRay(xform.MapID, ray, range, user, false).ToList();

        Vector2 targetPos;
        if (rayResults.Count > 0)
            targetPos = rayResults.MinBy(x => (x.HitPos - coords).Length()).HitPos - dir;
        else
            targetPos = coords + (msg.Direction.Length() > blink.Distance ? dir * blink.Distance : msg.Direction);

        _useDelay.TryResetDelay((weapon, delay), id: blink.BlinkDelay);
        _transform.SetWorldPosition(user, targetPos);
        _audio.PlayPredicted(blink.BlinkSound, user, user);
        if (_net.IsServer) // Prediction issues
            _telefrag.DoTelefrag(user, xform.Coordinates, blink.KnockdownTime);
    }
}
