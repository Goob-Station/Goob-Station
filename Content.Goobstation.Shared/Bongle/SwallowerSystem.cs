using Content.Shared.Body.Events;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Popups;
using Content.Shared.Throwing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;

namespace Content.Goobstation.Shared.Bongle;

public sealed partial class SwallowerSystem : EntitySystem
{
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private ThrowingSystem _throwing = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private SharedDamageOtherOnHitSystem _damageOtherOnHit = default!;

    [SubscribeLocalEvent]
    private void OnStartup(Entity<SwallowerComponent> ent, ref ComponentStartup args)
    {
        ent.Comp.Stomach = _container.EnsureContainer<Container>(ent.Owner, ent.Comp.ContainerId);
    }

    [SubscribeLocalEvent]
    private void OnPullAttempt(Entity<SwallowerComponent> ent, ref PullAttemptEvent args)
    {
        if (args.PullerUid != ent.Owner)
            return;

        args.Cancelled = true;

        if (ent.Comp.Stomach.ContainedEntities.Count >= ent.Comp.Capacity)
        {
            _popup.PopupPredicted(Loc.GetString("swallower-full"), ent.Owner, ent.Owner, PopupType.Medium);
            return;
        }

        if (!IsSwallowable(ent, args.PulledUid))
            return;

        var doAfterArgs = new DoAfterArgs(EntityManager, ent.Owner, ent.Comp.SwallowTime, new SwallowDoAfterEvent(), ent.Owner, target: args.PulledUid)
        {
            BreakOnMove = true,
            DuplicateCondition = DuplicateConditions.SameEvent
        };

        _doAfter.TryStartDoAfter(doAfterArgs);

        _popup.PopupPredicted(Loc.GetString("swallower-begin"), Loc.GetString("swallower-begin-others", ("user", Identity.Entity(ent.Owner, EntityManager))), ent.Owner, ent.Owner);
    }

    [SubscribeLocalEvent]
    private void OnSwallowDoAfter(Entity<SwallowerComponent> ent, ref SwallowDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Args.Target is not { } target)
            return;

        if (ent.Comp.Stomach.ContainedEntities.Count >= ent.Comp.Capacity)
            return;

        if (!IsSwallowable(ent, target))
            return;

        if (!_container.Insert(target, ent.Comp.Stomach))
            return;

        _popup.PopupPredicted(Loc.GetString("swallower-finish"), Loc.GetString("swallower-finish-others", ("user", Identity.Entity(ent.Owner, EntityManager))), ent.Owner, ent.Owner);

        _audio.PlayPredicted(ent.Comp.SwallowSound, ent.Owner, ent.Owner);

        args.Handled = true;
    }

    [SubscribeLocalEvent]
    private void OnRegurgitate(Entity<SwallowerComponent> ent, ref SwallowerRegurgitateEvent args)
    {
        if (args.Handled || ent.Comp.Stomach.ContainedEntities.Count == 0)
            return;

        _container.EmptyContainer(ent.Comp.Stomach);

        _popup.PopupPredicted(Loc.GetString("swallower-regurgitate"), Loc.GetString("swallower-regurgitate-others"
        , ("user", Identity.Entity(ent.Owner, EntityManager))), ent.Owner, ent.Owner);

        _audio.PlayPredicted(ent.Comp.RegurgitateSound, ent.Owner, ent.Owner);

        args.Handled = true;
    }

    [SubscribeLocalEvent]
    private void OnSpit(Entity<SwallowerComponent> ent, ref SwallowerSpitEvent args)
    {
        if (args.Handled || ent.Comp.Stomach.ContainedEntities.Count == 0)
            return;

        var selfMap = _transform.GetMapCoordinates(ent.Owner);
        var targetMap = _transform.ToMapCoordinates(args.Target);
        var direction = targetMap.Position - selfMap.Position;

        if (direction.Length() <= 0f)
            return;

        direction = direction.Normalized() * ent.Comp.SpitDistance;

        var spitCount = Math.Min(ent.Comp.SpitCount, ent.Comp.Stomach.ContainedEntities.Count);

        var spat = 0;

        for (var i = ent.Comp.Stomach.ContainedEntities.Count - 1; i >= 0 && spat < spitCount; i--)
        {
            var item = ent.Comp.Stomach.ContainedEntities[i];

            if (!_container.Remove(item, ent.Comp.Stomach))
                continue;

            var damageOther = EnsureComp<DamageOtherOnHitComponent>(item);
            _damageOtherOnHit.SetDamage((item, damageOther), ent.Comp.SpitDamage);

            var itemDirection = direction;

            if (spitCount > 1)
            {
                var offset = Angle.FromDegrees(ent.Comp.SpitSpread * ((float) spat / (spitCount - 1) - 0.5f));
                itemDirection = offset.RotateVec(direction);
            }

            _throwing.TryThrow(item, itemDirection, ent.Comp.SpitSpeed, ent.Owner);

            spat++;
        }

        if (spat == 0)
            return;

        _popup.PopupPredicted(Loc.GetString("swallower-spit"), Loc.GetString("swallower-spit-others", ("user", Identity.Entity(ent.Owner, EntityManager))), ent.Owner, ent.Owner);

        _audio.PlayPredicted(ent.Comp.SpitSound, ent.Owner, ent.Owner);

        args.Handled = true;
    }

    [SubscribeLocalEvent]
    private void OnGibbed(Entity<SwallowerComponent> ent, ref BeingGibbedEvent args)
    {
        _container.EmptyContainer(ent.Comp.Stomach);
    }

    private bool IsSwallowable(Entity<SwallowerComponent> ent, EntityUid target)
    {
        return !ent.Comp.IncapacitatedOnly || !HasComp<MobStateComponent>(target) || _mobState.IsIncapacitated(target);
    }
}
