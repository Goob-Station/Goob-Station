using System.Collections.Immutable;
using Content.Goobstation.Common.Grab;
using Content.Goobstation.Shared.GrabIntent;
using Content.Goobstation.Shared.Voidwalker.Abilities.Kidnap.Victim;
using Content.Goobstation.Shared.Voidwalker.Actions;
using Content.Goobstation.Shared.Voidwalker.Components;
using Content.Goobstation.Shared.Voidwalker.Objectives.Components;
using Content.Goobstation.Shared.Voidwalker.Spaced;
using Content.Goobstation.Shared.Voidwalker.Voided;
using Content.Shared.Administration.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Verbs;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Voidwalker.Abilities.Kidnap;

public sealed partial class VoidwalkerKidnappingSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly RejuvenateSystem _rejuvenate = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<VoidwalkerKidnappingComponent, VoidwalkerKidnapDoAfterEvent>(OnVoidwalkerKidnapDoAfter);
        SubscribeLocalEvent<VoidwalkerKidnappingComponent, GetVerbsEvent<InnateVerb>>(OnGetVerbs);
    }

    public void StartKidnap(EntityUid kidnapper, EntityUid target, VoidwalkerKidnappingComponent? kidnapping = null)
    {
        if (!Resolve(kidnapper, ref kidnapping))
            return;

        if (!HasComp<ActorComponent>(target))
        {
            var noActorPopup = Loc.GetString("voidwalker-no-actor", ("target", Name(target)));
            _popup.PopupClient(noActorPopup, target, kidnapper, PopupType.MediumCaution);

            return;
        }

        var kidnapBeginPopup = Loc.GetString("voidwalker-kidnap-begin", ("target", Name(target)), ("user", Name(kidnapper)));
        _popup.PopupPredicted(kidnapBeginPopup, target, target, PopupType.MediumCaution);

        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            kidnapper,
            kidnapping.KidnapDoAfterDuration,
            new VoidwalkerKidnapDoAfterEvent(),
            eventTarget: kidnapper,
            target: target)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            BlockDuplicate = true,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnVoidwalkerKidnapDoAfter(Entity<VoidwalkerKidnappingComponent> entity, ref VoidwalkerKidnapDoAfterEvent args)
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
        voidedComp.Kidnapper = entity.Owner;

        TrySendToShadowRealm(target);
    }

    private void OnGetVerbs(Entity<VoidwalkerKidnappingComponent> entity, ref GetVerbsEvent<InnateVerb> args)
    {
        var target = args.Target;

        if (!args.CanInteract
            || !args.CanAccess)
            return;

        if (TryComp<SpacedStatusComponent>(entity, out var spaced)
            && !spaced.IsInSpace)
            return;

        if (!TryComp<GrabbableComponent>(target, out var grabbable) || grabbable.GrabStage <= GrabStage.Soft)
            return;

        InnateVerb kidnapVerb = new()
        {
            Act = () => StartKidnap(entity, target),
            Text = Loc.GetString("voidwalker-kidnap-verb"),
            Message = Loc.GetString("voidwalker-kidnap-verb-text"),
            Icon = new SpriteSpecifier.Rsi(new ResPath("_Goobstation/Actions/voidwalker.rsi"), "kidnap"),
            Priority = 1,
        };

        args.Verbs.Add(kidnapVerb);

    }

    public bool TrySendToShadowRealm(EntityUid target, Entity<VoidwalkerKidnappingComponent>? sender = null)
    {
        var popup = Loc.GetString("voidwalker-kidnap-enter");
        _popup.PopupClient(popup, target, target, PopupType.SmallCaution);

        if (!TryComp<MindContainerComponent>(target, out var targetMindContainer)
            || !targetMindContainer.HasMind)
            return false;

        var targetMind = Comp<MindComponent>(targetMindContainer.Mind.Value);
        targetMind.PreventGhosting = true;

        var spawnPoints = EntityManager
            .GetAllComponents(typeof(VoidedSpawnComponent))
            .ToImmutableList();

        if (spawnPoints.IsEmpty)
            return false;


        if (_net.IsServer)
        {
            var newSpawn = _random.Pick(spawnPoints);
            var spawnTarget = Transform(newSpawn.Uid).Coordinates;

            _transform.SetCoordinates(target, spawnTarget);
            _rejuvenate.PerformRejuvenate(target);

            var stunDuration = TimeSpan.FromSeconds(5);
            if (sender is { } kidnapper)
                stunDuration = kidnapper.Comp.KidnapStunDuration;

            _stun.KnockdownOrStun(target, stunDuration);

            // need more sfx here later
        }




        return true;
    }


}
