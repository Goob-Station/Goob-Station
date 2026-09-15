using System.Collections.Immutable;
using Content.Goobstation.Shared.Voidwalker.Abilities.Kidnap.Victim;
using Content.Goobstation.Shared.Voidwalker.Actions;
using Content.Goobstation.Shared.Voidwalker.Objectives.Components;
using Content.Goobstation.Shared.Voidwalker.Voided;
using Content.Shared.Administration.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;

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

    public override void Initialize()
    {
        SubscribeLocalEvent<VoidwalkerKidnappingComponent, VoidwalkerKidnapDoAfterEvent>(OnVoidwalkerKidnapDoAfter);
    }

    public void StartKidnap(Entity<VoidwalkerKidnappingComponent> entity, EntityUid target)
    {
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
            entity.Comp.KidnapDoAfterDuration,
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

    public bool TrySendToShadowRealm(EntityUid target)
    {
        var popup = Loc.GetString("voidwalker-kidnap-enter");
        _popup.PopupEntity(popup, target, target, PopupType.SmallCaution);

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

        var newSpawn = _random.Pick(spawnPoints);
        var spawnTarget = Transform(newSpawn.Uid).Coordinates;

        _transform.SetCoordinates(target, spawnTarget);
        _rejuvenate.PerformRejuvenate(target);
        _stun.KnockdownOrStun(target, TimeSpan.FromSeconds(5), true);
        // need more sfx here later

        return true;
    }
}
