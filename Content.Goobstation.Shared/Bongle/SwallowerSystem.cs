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

/// <summary>
/// Stfu about ERP for the love of god.
/// </summary>

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

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SwallowerComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<SwallowerComponent, PullAttemptEvent>(OnPullAttempt);
        SubscribeLocalEvent<SwallowerComponent, SwallowDoAfterEvent>(OnSwallowDoAfter);
        SubscribeLocalEvent<SwallowerComponent, SwallowerSpitEvent>(OnSpit);
        SubscribeLocalEvent<SwallowerComponent, SwallowerRegurgitateEvent>(OnRegurgitate);
        SubscribeLocalEvent<SwallowerComponent, BeingGibbedEvent>(OnGibbed);
    }

    private void OnStartup(Entity<SwallowerComponent> ent, ref ComponentStartup args)
    {
        var (uid, comp) = ent;

        comp.Stomach = _container.EnsureContainer<Container>(uid, comp.ContainerId);
    }

    private void OnPullAttempt(Entity<SwallowerComponent> ent, ref PullAttemptEvent args)
    {
        var (uid, comp) = ent;

        if (args.PullerUid != uid)
            return;

        args.Cancelled = true;

        if (comp.Stomach.ContainedEntities.Count >= comp.Capacity)
        {
            _popup.PopupPredicted(Loc.GetString("swallower-full"), uid, uid, PopupType.Medium);
            return;
        }

        if (!IsSwallowable(comp, args.PulledUid))
            return;

        var doAfterArgs = new DoAfterArgs(EntityManager, uid, comp.SwallowTime, new SwallowDoAfterEvent(), uid, target: args.PulledUid)
        {
            BreakOnMove = true,
            DuplicateCondition = DuplicateConditions.SameEvent
        };
        _doAfter.TryStartDoAfter(doAfterArgs);

        _popup.PopupPredicted(Loc.GetString("swallower-begin"), Loc.GetString("swallower-begin-others", ("user", Identity.Entity(uid, EntityManager))), uid, uid);
    }

    private void OnSwallowDoAfter(Entity<SwallowerComponent> ent, ref SwallowDoAfterEvent args)
    {
        var (uid, comp) = ent;

        if (args.Handled || args.Cancelled || args.Args.Target is not { } target)
            return;

        if (comp.Stomach.ContainedEntities.Count >= comp.Capacity)
            return;

        if (!IsSwallowable(comp, target))
            return;

        if (!_container.Insert(target, comp.Stomach))
            return;

        _popup.PopupPredicted(Loc.GetString("swallower-finish"), Loc.GetString("swallower-finish-others", ("user", Identity.Entity(uid, EntityManager))), uid, uid);

        _audio.PlayPredicted(comp.SwallowSound, uid, uid);

        args.Handled = true;
    }

    private void OnRegurgitate(Entity<SwallowerComponent> ent, ref SwallowerRegurgitateEvent args)
    {
        var (uid, comp) = ent;

        if (args.Handled || comp.Stomach.ContainedEntities.Count == 0)
            return;

        _container.EmptyContainer(comp.Stomach);

        _popup.PopupPredicted(Loc.GetString("swallower-regurgitate"), Loc.GetString("swallower-regurgitate-others", ("user", Identity.Entity(uid, EntityManager))), uid, uid);
        _audio.PlayPredicted(comp.RegurgitateSound, uid, uid);

        args.Handled = true;
    }

    private void OnSpit(Entity<SwallowerComponent> ent, ref SwallowerSpitEvent args)
    {
        var (uid, comp) = ent;

        if (args.Handled || comp.Stomach.ContainedEntities.Count == 0)
            return;

        var selfMap = _transform.GetMapCoordinates(uid);
        var targetMap = _transform.ToMapCoordinates(args.Target);
        var direction = targetMap.Position - selfMap.Position;

        if (direction.Length() <= 0f)
            return;

        direction = direction.Normalized() * comp.SpitDistance;

        var spitCount = Math.Min(comp.SpitCount, comp.Stomach.ContainedEntities.Count);

        var spat = 0;

        for (var i = comp.Stomach.ContainedEntities.Count - 1; i >= 0 && spat < spitCount; i--)
        {
            var item = comp.Stomach.ContainedEntities[i];

            if (!_container.Remove(item, comp.Stomach))
                continue;

            var damageOther = EnsureComp<DamageOtherOnHitComponent>(item);
            _damageOtherOnHit.SetDamage((item, damageOther), comp.SpitDamage);

            var itemDirection = direction;

            if (spitCount > 1)
            {
                var offset = Angle.FromDegrees(comp.SpitSpread * ((float) spat / (spitCount - 1) - 0.5f));
                itemDirection = offset.RotateVec(direction);
            }

            _throwing.TryThrow(item, itemDirection, comp.SpitSpeed, uid);

            spat++;
        }

        if (spat == 0)
            return;

        _popup.PopupPredicted(Loc.GetString("swallower-spit"), Loc.GetString("swallower-spit-others", ("user", Identity.Entity(uid, EntityManager))), uid, uid);

        _audio.PlayPredicted(comp.SpitSound, uid, uid);

        args.Handled = true;
    }

    private void OnGibbed(Entity<SwallowerComponent> ent, ref BeingGibbedEvent args)
    {
        var (_, comp) = ent;

        _container.EmptyContainer(comp.Stomach);
    }

    private bool IsSwallowable(SwallowerComponent comp, EntityUid target)
    {
        return !comp.IncapacitatedOnly || !HasComp<MobStateComponent>(target) || _mobState.IsIncapacitated(target);
    }
}
