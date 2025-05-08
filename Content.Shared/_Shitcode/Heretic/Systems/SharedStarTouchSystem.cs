using System.Linq;
using Content.Shared._Goobstation.Wizard.FadingTimedDespawn;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Bed.Sleep;
using Content.Shared.Damage;
using Content.Shared.Examine;
using Content.Shared.Heretic;
using Content.Shared.Interaction;
using Content.Shared.Magic;
using Content.Shared.Physics;
using Content.Shared.StatusEffect;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;

namespace Content.Shared._Shitcode.Heretic.Systems;

public abstract class SharedStarTouchSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;

    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly ExamineSystemShared _examine = default!;
    [Dependency] private readonly SharedMagicSystem _magic = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedStarMarkSystem _starMark = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StarTouchComponent, AfterInteractEvent>(OnAfterInteract);

        SubscribeLocalEvent<ContinuousBeamComponent, StarTouchBeamEvent>(OnBeamTick);
    }

    private void OnBeamTick(Entity<ContinuousBeamComponent> ent, ref StarTouchBeamEvent args)
    {
        if (!TryGetEntity(args.User, out var user) || ent.Owner != user.Value)
            return;

        args.Handled = true;

        if (!TryGetEntity(args.Target, out var target) || !TryComp(target.Value, out TransformComponent? targetXform) ||
            !targetXform.Coordinates.IsValid(EntityManager) || !HasComp<HereticComponent>(user.Value))
        {
            BreakBeam(args.Target);
            return;
        }

        var coords = _transform.GetMapCoordinates(user.Value);
        var targetCoords = _transform.GetMapCoordinates(target.Value, targetXform);

        if (coords.MapId != targetCoords.MapId)
        {
            BreakBeam(args.Target);
            return;
        }

        if (!_examine.InRangeUnOccluded(user.Value, target.Value))
        {
            BreakBeam(args.Target);
            return;
        }

        _damageable.TryChangeDamage(target.Value, args.Damage, origin: user.Value, targetPart: TargetBodyPart.Torso);

        return;

        void BreakBeam(NetEntity netTarget)
        {
            if (_net.IsClient)
                return;

            ent.Comp.Data.Remove(netTarget);
            if (ent.Comp.Data.Count == 0)
                RemCompDeferred(ent.Owner, ent.Comp);
            else
                Dirty(ent);
        }
    }

    private void OnAfterInteract(Entity<StarTouchComponent> ent, ref AfterInteractEvent args)
    {
        if (!args.CanReach)
            return;

        if (args.Target == null || args.Target == args.User)
            return;

        var (uid, comp) = ent;

        var target = args.Target.Value;

        if (!TryComp(target, out StatusEffectsComponent? status))
            return;

        args.Handled = true;

        if (!TryComp<HereticComponent>(args.User, out var hereticComp) ||
            TryComp<HereticComponent>(target, out var th) && th.CurrentPath == hereticComp.CurrentPath)
        {
            if (_net.IsServer)
                QueueDel(uid);
            return;
        }

        if (_magic.SpellDenied(target))
        {
            InvokeSpell(ent, args.User);
            return;
        }

        var range = hereticComp.Ascended ? 2 : 1;
        var xform = Transform(args.User);
        _starMark.SpawnCosmicFieldLine(xform.Coordinates,
            Angle.FromDegrees(90f).RotateDir(xform.LocalRotation.GetDir()).AsFlag(),
            -1,
            1,
            1);

        if (HasComp<StarMarkComponent>(target))
        {
            if (_status.TryAddStatusEffect<ForcedSleepingComponent>(target,
                    "ForcedSleep",
                    comp.SleepTime,
                    true,
                    status))
                _status.TryRemoveStatusEffect(target, "StarMark", status);
        }
        else
            _starMark.TryApplyStarMark(target, status);

        var beam = EnsureComp<ContinuousBeamComponent>(args.User);
        var netTarget = GetNetEntity(args.Target.Value);
        beam.Data.Remove(netTarget);
        beam.Data.Add(netTarget,
            new ContinuousBeamData(
            ent.Comp.BeamSprite,
            ent.Comp.BeamLifetime,
            ent.Comp.BeamTickInterval,
            ent.Comp.BeamMaxDistanceSquared,
            Color.White,
            new StarTouchBeamEvent()));
        Dirty(args.User, beam);

        InvokeSpell(ent, args.User);
    }

    public virtual void InvokeSpell(Entity<StarTouchComponent> ent, EntityUid user, bool deleteSpell = true)
    {
        _audio.PlayPredicted(ent.Comp.Sound, user, user);
    }
}
