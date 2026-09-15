using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Events;
using Content.Shared.Popups;
using Content.Shared._White.Xenomorphs.Egg;
using Content.Shared._White.Xenomorphs.HiveAnnounce;
using Content.Shared._White.Xenomorphs.Ovipositor;
using Content.Server.DoAfter;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Server.GameObjects;

namespace Content.Server._White.Xenomorphs.Ovipositor;

public sealed class XenomorphOvipositorSystem : EntitySystem
{
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<XenomorphOvipositorComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<XenomorphOvipositorComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<XenomorphOvipositorComponent, XenomorphOvipositorAttachEvent>(OnAttachAction);
        SubscribeLocalEvent<XenomorphOvipositorComponent, XenomorphOvipositorDetachEvent>(OnDetachAction);
        SubscribeLocalEvent<XenomorphOvipositorComponent, XenomorphOvipositorAttachDoAfterEvent>(OnAttachDoAfter);
        SubscribeLocalEvent<XenomorphOvipositorComponent, XenomorphOvipositorDetachDoAfterEvent>(OnDetachDoAfter);
        SubscribeLocalEvent<XenomorphOvipositorComponent, UpdateCanMoveEvent>(OnUpdateCanMove);
        SubscribeLocalEvent<XenomorphOvipositorComponent, DamageChangedEvent>(OnDamage);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<XenomorphOvipositorComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (!component.Attached || !_mobState.IsAlive(uid))
                continue;

            if (now < component.NextLayEggAt)
                continue;

            LayEggBeside(uid, component);
        }
    }

    private void OnDamage(EntityUid uid, XenomorphOvipositorComponent component, DamageChangedEvent args)
    {
        // Any damage while attached: stand up immediately (no detach do-after) + burst VFX.
        if (!component.Attached || !args.DamageIncreased)
            return;

        if (component.BurstEffectPrototype is { } burstProto)
            Spawn(burstProto, Transform(uid).Coordinates);

        SetAttached(uid, component, false);
        _popup.PopupEntity(Loc.GetString("xenomorphs-ovipositor-detached-damage"), uid, uid);
    }

    private void OnMapInit(EntityUid uid, XenomorphOvipositorComponent component, MapInitEvent args)
    {
        _actions.AddAction(uid, ref component.AttachAction, component.AttachActionId);
        EnsureComp<AppearanceComponent>(uid);
        _appearance.SetData(uid, XenomorphOvipositorVisuals.Attached, false);
        // Hive announce is granted to anyone with an ovipositor.
        EnsureComp<XenomorphHiveAnnounceComponent>(uid);
    }

    private void OnShutdown(EntityUid uid, XenomorphOvipositorComponent component, ComponentShutdown args)
    {
        _actions.RemoveAction(uid, component.AttachAction);
        _actions.RemoveAction(uid, component.DetachAction);
        _actions.RemoveAction(uid, component.LayEggAction);
        RemComp<XenomorphHiveAnnounceComponent>(uid);
    }

    private void OnUpdateCanMove(EntityUid uid, XenomorphOvipositorComponent component, UpdateCanMoveEvent args)
    {
        if (component.Attached)
            args.Cancel();
    }

    private void OnAttachAction(EntityUid uid, XenomorphOvipositorComponent component, XenomorphOvipositorAttachEvent args)
    {
        if (args.Handled || component.Attached || !_mobState.IsAlive(uid))
            return;

        var doAfter = new DoAfterArgs(EntityManager, uid, component.AttachDelay, new XenomorphOvipositorAttachDoAfterEvent(), uid)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            NeedHand = false,
            // Rooted/static bodies fail default CanInteract checks and would soft-lock detach/attach.
            RequireCanInteract = false,
            CancelDuplicate = true,
            BlockDuplicate = true,
        };

        if (_doAfter.TryStartDoAfter(doAfter))
            args.Handled = true;
    }

    private void OnDetachAction(EntityUid uid, XenomorphOvipositorComponent component, XenomorphOvipositorDetachEvent args)
    {
        if (args.Handled || !component.Attached)
            return;

        var doAfter = new DoAfterArgs(EntityManager, uid, component.DetachDelay, new XenomorphOvipositorDetachDoAfterEvent(), uid)
        {
            // Damage already force-detaches via OnDamage; BreakOnDamage would cancel stand-up mid-channel.
            BreakOnDamage = false,
            BreakOnMove = false,
            NeedHand = false,
            RequireCanInteract = false,
            CancelDuplicate = true,
            BlockDuplicate = true,
        };

        if (_doAfter.TryStartDoAfter(doAfter))
            args.Handled = true;
    }

    private void OnAttachDoAfter(EntityUid uid, XenomorphOvipositorComponent component, XenomorphOvipositorAttachDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || component.Attached)
            return;

        args.Handled = true;
        SetAttached(uid, component, true);
        _popup.PopupEntity(Loc.GetString("xenomorphs-ovipositor-attached"), uid, uid);
    }

    private void OnDetachDoAfter(EntityUid uid, XenomorphOvipositorComponent component, XenomorphOvipositorDetachDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || !component.Attached)
            return;

        args.Handled = true;
        SetAttached(uid, component, false);
        _popup.PopupEntity(Loc.GetString("xenomorphs-ovipositor-detached"), uid, uid);
    }

    private void LayEggBeside(EntityUid uid, XenomorphOvipositorComponent component)
    {
        // Always advance cooldown so we don't spin every frame at the cap.
        component.NextLayEggAt = _timing.CurTime + component.LayEggCooldown;
        Dirty(uid, component);

        if (CountNearbyUnplantedEggs(uid, component) >= component.MaxNearbyUnplantedEggs)
            return;

        var coords = Transform(uid).Coordinates;
        // Drop beside the empress (not into hands).
        Spawn(component.EggPrototype, coords.Offset(_random.NextVector2(0.6f, 1.1f)));
    }

    private int CountNearbyUnplantedEggs(EntityUid uid, XenomorphOvipositorComponent component)
    {
        var count = 0;
        foreach (var egg in _lookup.GetEntitiesInRange<XenomorphPlantableEggComponent>(
                     _transform.GetMapCoordinates(uid), component.NearbyEggRange))
        {
            if (MetaData(egg).EntityPrototype?.ID == component.EggPrototype.Id)
                count++;
        }

        return count;
    }

    private void SetAttached(EntityUid uid, XenomorphOvipositorComponent component, bool attached)
    {
        component.Attached = attached;
        Dirty(uid, component);
        _appearance.SetData(uid, XenomorphOvipositorVisuals.Attached, attached);

        TryComp(uid, out PhysicsComponent? physics);

        if (attached)
        {
            // Static first, then anchor (anchored mobs expect Static).
            if (physics != null)
                _physics.SetBodyType(uid, BodyType.Static, body: physics);
            _transform.AnchorEntity(uid);

            _actions.AddAction(uid, ref component.DetachAction, component.DetachActionId);
            _actions.RemoveAction(uid, component.AttachAction);
            component.AttachAction = null;
            // First egg after one cooldown tick while rooted.
            component.NextLayEggAt = _timing.CurTime + component.LayEggCooldown;
            Dirty(uid, component);
        }
        else
        {
            // Unanchor BEFORE restoring KinematicController — otherwise body can stay Static
            // and InputMover keeps cancelling movement (stuck on ovipositor).
            _transform.Unanchor(uid);
            if (physics != null)
                _physics.SetBodyType(uid, BodyType.KinematicController, body: physics);

            _actions.RemoveAction(uid, component.DetachAction);
            _actions.RemoveAction(uid, component.LayEggAction);
            component.DetachAction = null;
            component.LayEggAction = null;
            _actions.AddAction(uid, ref component.AttachAction, component.AttachActionId);
        }

        _actionBlocker.UpdateCanMove(uid);
    }
}
