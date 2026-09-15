using System.Collections.Immutable;
using Content.Goobstation.Common.VoidedVisualizer;
using Content.Goobstation.Server.Voidwalker.Kidnapping;
using Content.Goobstation.Server.Voidwalker.Kidnapping.Voided;
using Content.Goobstation.Server.Voidwalker.Objectives.Components;
using Content.Goobstation.Shared.Dash;
using Content.Goobstation.Shared.Voidwalker;
using Content.Goobstation.Shared.Voidwalker.Actions;
using Content.Shared.Chat.Prototypes;
using Content.Shared.DoAfter;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Popups;
using Content.Shared.Random.Helpers;
using Robust.Server.Toolshed.Commands.Players;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Voidwalker;

public sealed partial class VoidwalkerSystem
{
    private static ProtoId<EmotePrototype> Scream = "Scream";
    private void SubscribeAbilities()
    {
        SubscribeLocalEvent<VoidwalkerComponent, VoidwalkerUnsettleEvent>(OnUnsettle);
        SubscribeLocalEvent<VoidwalkerComponent, VoidwalkerUnsettleDoAfterEvent>(OnUnsettleDoAfter);

        SubscribeLocalEvent<VoidwalkerComponent, VoidwalkerKidnapDoAfterEvent>(OnVoidwalkerKidnapDoAfter);

        SubscribeLocalEvent<VoidwalkerComponent, VoidwalkerVoidWalkEvent>(OnVoidWalk);

        SubscribeLocalEvent<VoidwalkerComponent, VoidwalkerConvertWallDoAfterEvent>(OnConvertWallDoAfter);

        SubscribeLocalEvent<ExitNebulaCrawlComponent, ExitNebulaCrawlEvent>(OnExitNebulaCrawl);
    }

    private void OnUnsettle(Entity<VoidwalkerComponent> entity, ref VoidwalkerUnsettleEvent args)
    {
        var target = args.Target;

        if (!TryUseAbility(entity, args))
            return;

        if (_mobState.IsIncapacitated(target))
        {
            _popup.PopupEntity(Loc.GetString("voidwalker-unsettle-fail-incapacitated"), target, entity);
            return;
        }

        if (!CanSeeVoidwalker(target))
        {
            _popup.PopupEntity(Loc.GetString("voidwalker-unsettle-fail-blind"), target, entity);
            return;
        }

        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            entity,
            entity.Comp.UnsettleDoAfterDuration,
            new VoidwalkerUnsettleDoAfterEvent(),
            eventTarget: entity,
            target: target)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            RequireCanInteract = false, // use your EYES!!!!
            DistanceThreshold = 20,
            IgnoreObstruction = true,
            Hidden = true,
        };

        if (!_doAfter.TryStartDoAfter(doAfterArgs, out var id))
            return;

        entity.Comp.UnsettleDoAfterId = id.Value.Index;

        var popup = Loc.GetString("voidwalker-unsettle-begin", ("target", Name(target)));
        _popup.PopupEntity(popup, entity, entity, PopupType.Medium);
    }

    private void OnUnsettleDoAfter(Entity<VoidwalkerComponent> entity, ref VoidwalkerUnsettleDoAfterEvent args)
    {
        entity.Comp.UnsettleDoAfterId = null;

        if (args.Target is not { } target
            || args.Cancelled
            || args.Handled)
            return;

        args.Handled = true;

        _stun.KnockdownOrStun(target, entity.Comp.UnsettleStunDuration, true);
        _chat.TryEmoteWithChat(target, Scream);
        _stamina.TakeStaminaDamage(target, entity.Comp.UnsettleStaminaDamage);
        _slurred.DoSlur(target, entity.Comp.UnsettleStunDuration * 2);

        var popup = Loc.GetString("voidwalker-unsettle-victim");
        _popup.PopupEntity(popup, target, target, PopupType.LargeCaution);
    }

    public void StartKidnap(EntityUid entity, EntityUid target)
    {
        if (!TryComp<VoidwalkerComponent>(entity, out var voidwalkerComponent))
            return;

        if (HasComp<ActorComponent>(target))
        {
            var noActorPopup = Loc.GetString("voidwalker-no-actor", ("target", Name(target)));
            _popup.PopupEntity(noActorPopup, target, entity, PopupType.MediumCaution);

            return;
        }


        var kidnapBeginPopup = Loc.GetString("voidwalker-kidnap-begin", ("target", Name(target)), ("user", Name(entity)));
        _popup.PopupEntity(kidnapBeginPopup, target, PopupType.MediumCaution);

        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            entity,
            voidwalkerComponent.KidnapDoAfterDuration,
            new VoidwalkerKidnapDoAfterEvent(),
            eventTarget: entity,
            target: target)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            BlockDuplicate = true,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnVoidwalkerKidnapDoAfter(Entity<VoidwalkerComponent> entity, ref VoidwalkerKidnapDoAfterEvent args)
    {
        if (args.Target is not { } target
            || args.Cancelled
            || args.Handled)
            return;

        args.Handled = true;

        if (_mind.TryGetMind(entity, out var voidwalkerMindId, out var voidwalkerMind)
            && _mind.TryGetObjectiveComp<VoidwalkerKidnapConditionComponent>(voidwalkerMindId, out var objective, voidwalkerMind))
            objective.Kidnapped += 1;

        if (!TryComp<MindContainerComponent>(target, out var targetMindContainer)
            || !targetMindContainer.HasMind)
            return;

        var originalMapId = Transform(target).MapID;
        var originalMapUid = _map.GetMap(originalMapId);

        var kidnappedComp = EnsureComp<VoidwalkerKidnappedComponent>(target);
        kidnappedComp.ExitVoidTime = _timing.CurTime + entity.Comp.KidnapDuration;
        kidnappedComp.OriginalMap = originalMapUid;

        var voidedComp = EnsureComp<VoidedComponent>(target);
        voidedComp.Voidwalker = entity;

        TrySendToShadowRealm(target);
    }

    private void OnVoidWalk(Entity<VoidwalkerComponent> entity, ref VoidwalkerVoidWalkEvent args)
    {
        if (!TryUseAbility(entity, args))
            return;

        var vec = (_transform.ToMapCoordinates(args.Target).Position -
                   _transform.GetMapCoordinates(args.Performer).Position).Normalized() * args.Distance;
        var speed = args.Speed;

        _throwing.TryThrow(args.Performer, vec, speed, animated: false);
        _stealth.SetVisibility(entity, -1);
    }

    private void StartConvertWall(Entity<VoidwalkerComponent> entity, EntityUid target)
    {
        var popup = Loc.GetString("voidwalker-convert-wall-begin", ("user", Name(entity.Owner)));
        _popup.PopupEntity(popup, target, PopupType.SmallCaution);

        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            entity,
            entity.Comp.WallConvertTime,
            new VoidwalkerConvertWallDoAfterEvent(),
            eventTarget: entity,
            target: target)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            BlockDuplicate = true,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnConvertWallDoAfter(Entity<VoidwalkerComponent> entity, ref VoidwalkerConvertWallDoAfterEvent args)
    {
        if (args.Target is not { } target
            || args.Cancelled
            || args.Handled)
            return;

        args.Handled = true;

        EnsureComp<VoidedVisualsComponent>(target);
        _tag.AddTag(target, entity.Comp.VoidedStructureTag);
    }

    private void OnExitNebulaCrawl(Entity<ExitNebulaCrawlComponent> entity, ref ExitNebulaCrawlEvent args)
    {
        if (args.Handled)
            return;

        if (!CheckInSpace(entity))
        {
            var popup = Loc.GetString("voidwalker-action-fail-require-in-space");
            _popup.PopupEntity(popup, entity, entity);

            return;
        }

        _polymorph.Revert(entity.Owner);

        args.Handled = true;
    }

}
