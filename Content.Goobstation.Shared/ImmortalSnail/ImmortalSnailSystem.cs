using Content.Shared.Actions;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Localizations;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Traits.Assorted;
using Content.Shared.Mind;
using Robust.Shared.Map;

namespace Content.Goobstation.Shared.ImmortalSnail;

public abstract class SharedImmortalSnailSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly IMapManager _mapMan = default!;
    [Dependency] private readonly SharedGodmodeSystem _godmode = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ImmortalSnailComponent, MapInitEvent>(OnSnailMapInit);
        SubscribeLocalEvent<ImmortalSnailComponent, ComponentShutdown>(OnSnailShutdown);
        SubscribeLocalEvent<ImmortalSnailComponent, TouchOfDeathEvent>(OnTouchOfDeath);
        SubscribeLocalEvent<ImmortalSnailComponent, SnailHeartbeatEvent>(OnSnailHeartbeat);
    }

    private void OnSnailMapInit(Entity<ImmortalSnailComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.TouchOfDeathActionEntity = _actions.AddAction(ent, ent.Comp.TouchOfDeathAction);
        ent.Comp.HeartbeatActionEntity = _actions.AddAction(ent, ent.Comp.HeartbeatAction);
    }

    private void OnSnailShutdown(Entity<ImmortalSnailComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent.Comp.TouchOfDeathActionEntity);
        _actions.RemoveAction(ent.Comp.HeartbeatActionEntity);
    }

    private void OnSnailHeartbeat(Entity<ImmortalSnailComponent> ent, ref SnailHeartbeatEvent args)
    {
        if (args.Handled)
            return;

        if (ent.Comp.Target == null || !Exists(ent.Comp.Target.Value))
        {
            _popup.PopupEntity(Loc.GetString("immortal-snail-no-target"), ent, ent, PopupType.Medium);
            args.Handled = true;
            return;
        }

        var target = ent.Comp.Target.Value;

        var ourMapCoords = _transform.GetMapCoordinates(ent);
        var targetMapCoords = _transform.GetMapCoordinates(target);

        string loc;

        if (_map.IsPaused(targetMapCoords.MapId))
            loc = Loc.GetString("immortal-snail-heartbeat-unknown");
        else if (targetMapCoords.MapId != ourMapCoords.MapId)
            loc = Loc.GetString("immortal-snail-heartbeat-faraway");
        else
        {
            var targetStation = GetOwningStation(target);
            var ownStation = GetOwningStation(ent);

            var isOnStation = targetStation != null && targetStation == ownStation;

            var ang = Angle.Zero;
            if (_mapMan.TryFindGridAt(_transform.GetMapCoordinates(Transform(ent)), out var grid, out var _))
                ang = Transform(grid).LocalRotation;

            var vector = targetMapCoords.Position - ourMapCoords.Position;
            var direction = (vector.ToWorldAngle() - ang).GetDir();

            var locdir = ContentLocalizationManager.FormatDirection(direction).ToLower();

            loc = Loc.GetString(isOnStation ? "immortal-snail-heartbeat-onstation" : "immortal-snail-heartbeat-offstation",
                ("direction", locdir));
        }

        _popup.PopupEntity(loc, ent, ent, PopupType.Medium);

        args.Handled = true;
    }

    private void OnTouchOfDeath(Entity<ImmortalSnailComponent> ent, ref TouchOfDeathEvent args)
    {
        if (args.Handled)
            return;

        if (!Exists(args.Target))
        {
            _popup.PopupCursor(Loc.GetString("immortal-snail-no-target"), ent, PopupType.Medium);
            args.Handled = true;
            return;
        }

        var target = args.Target;

        if (ent.Comp.Target == null
            || target != ent.Comp.Target.Value)
        {
            _popup.PopupEntity(Loc.GetString("immortal-snail-touch-wrong-target"), ent, ent, PopupType.MediumCaution);
            args.Handled = true;
            return;
        }

        if (TryComp<MobStateComponent>(target, out var mobState)
            && _mobState.IsDead(target, mobState))
        {
            _popup.PopupEntity(Loc.GetString("immortal-snail-touch-already-dead"), ent, ent, PopupType.Medium);
            args.Handled = true;
            return;
        }

        if (TryComp<MobStateComponent>(target, out mobState))
        {
            var targetName = MetaData(target).EntityName;

            OnTouchOfDeathBuffs(ent, targetName);

            if (HasComp<GodmodeComponent>(ent))
                _godmode.DisableGodmode(ent);

            if (HasComp<GodmodeComponent>(target))
                _godmode.DisableGodmode(target);

            if (HasComp<ImmortalSnailTargetComponent>(target))
                RemCompDeferred<ImmortalSnailTargetComponent>(target);

            var isSmited = false;
            if (_mind.TryGetMind(target, out var targetMind, out var mindComp))
                foreach (var roleId in mindComp.MindRoles)
                    if (HasComp<ImmortalSnailSmitedComponent>(roleId))
                    {
                        isSmited = true;
                        break;
                    }

            if (!isSmited)
            {
                var unrevivable = EnsureComp<UnrevivableComponent>(target);
                unrevivable.ReasonMessage = "immortal-snail-unrevivable";
            }

            if (_mind.TryGetMind(target, out var mindId, out _))
                _mind.WipeMind(mindId);

            _mobState.ChangeMobState(target, MobState.Dead, mobState);

            _popup.PopupEntity(Loc.GetString("immortal-snail-touch-kill-self", ("target", targetName)), ent, ent);

            _popup.PopupEntity(Loc.GetString("immortal-snail-touch-kill-target"), target, target, PopupType.LargeCaution);

            args.Handled = true;
        }
    }

    protected virtual void OnTouchOfDeathBuffs(EntityUid snail, string targetName)
    {
    }

    protected virtual EntityUid? GetOwningStation(EntityUid entity)
    {
        return null;
    }
}
