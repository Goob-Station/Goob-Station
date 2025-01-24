using Content.Shared.Ghost.Roles.Components;
using Content.Shared.StepTrigger.Systems;
using Content.Shared.Mobs.Components;
using Robust.Shared.Containers;
using Content.Shared.Destructible;
using Content.Shared._Goobstation.Bingle;
using Content.Shared.Stunnable;
using Content.Server.Stunnable;
using Content.Shared.Humanoid;
using Robust.Shared.Audio.Systems;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Network;
using Robust.Shared.Timing;
using Content.Shared.Movement.Events;

namespace Content.Server._Goobstation.Bingle;

public sealed class BinglePitSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly BingleSystem _bingleSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BinglePitComponent, StepTriggeredOffEvent>(OnStepTriggered);
        SubscribeLocalEvent<BinglePitComponent, StepTriggerAttemptEvent>(OnStepTriggerAttempt);
        SubscribeLocalEvent<BinglePitComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<BinglePitComponent, DestructionEventArgs>(OnDestruction);
        SubscribeLocalEvent<BinglePitComponent, AttackedEvent>(OnAttacked);
        SubscribeLocalEvent<BinglePitFallingComponent, UpdateCanMoveEvent>(OnUpdateCanMove);
    }
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<BinglePitFallingComponent>();
        while (query.MoveNext(out var uid, out var falling))
        {
            if (_timing.CurTime < falling.NextDeletionTime)
                continue;

            //QueueDel(uid);

            _containerSystem.Insert(uid, falling.Pit.Pit);
            EnsureComp<StunnedComponent>(uid); // used stuned to prevent any funny being done inside the pit
            RemComp<BinglePitFallingComponent>(uid);

        }
    }
    private void OnInit(EntityUid uid, BinglePitComponent component, MapInitEvent args)
    {
        component.Pit = _containerSystem.EnsureContainer<Container>(uid, "pit");
    }
    private void OnStepTriggered(EntityUid uid, BinglePitComponent component, ref StepTriggeredOffEvent args)
    {
        // dont swallow bingles
        if (HasComp<BingleComponent>(args.Tripper))
            return;
        // need to be at levl 1 or above to swallow anything alive
        if (HasComp<MobStateComponent>(args.Tripper) && component.Level == 0)
            return;
        if (HasComp<BinglePitFallingComponent>(args.Tripper))
            return;

        StartFalling(uid, component, args.Tripper);

        if (component.BinglePoints >= component.SpawnNewAt)
        {
            SpawnBingle(uid, component);
            component.BinglePoints = component.BinglePoints - component.SpawnNewAt;
        }
    }
    public void StartFalling(EntityUid uid, BinglePitComponent component, EntityUid tripper, bool playSound = true)
    {
        if (HasComp<MobStateComponent>(tripper))
        {
            component.BinglePoints = component.BinglePoints + 5f;
            if (HasComp<HumanoidAppearanceComponent>(tripper))
                component.BinglePoints = component.BinglePoints + 5f;
        }
        else
            component.BinglePoints++;

        var fall= EnsureComp<BinglePitFallingComponent>(tripper);
        fall.Pit= component;
        fall.NextDeletionTime = _timing.CurTime + fall.DeletionTime;
        _stun.TryKnockdown(tripper, fall.DeletionTime, false);

        if (playSound)
            _audio.PlayPredicted(component.FallingSound, uid, tripper);

    }
    private void OnStepTriggerAttempt(EntityUid uid, BinglePitComponent component, ref StepTriggerAttemptEvent args)
    {
        args.Continue = true;
    }
    public void SpawnBingle(EntityUid uid, BinglePitComponent component)
    {
        Spawn("SpawnPointGhostBingle", Transform(uid).Coordinates);

        component.MinionsMade++;
        if (component.MinionsMade >= component.UpgradeMinionsAfter)
        {
            component.MinionsMade = 0;
            component.Level++;
            UpgradeBingles(uid, component);
        }
    }
    public void UpgradeBingles(EntityUid uid, BinglePitComponent component)
    {
        var query = EntityQueryEnumerator<BingleComponent>();
        while (query.MoveNext(out var queryUid, out var queryBingleComp))
        {
            _bingleSystem.UpgradeBingle(queryUid, queryBingleComp);
        }
        if (component.Level <= component.MaxSize)
            RaiseNetworkEvent(new BinglePitGrowEvent(GetNetEntity(uid), component.Level));
    }
    private void OnDestruction(EntityUid uid, BinglePitComponent component, DestructionEventArgs args)
    {
        if (component.Pit != null)
        {
            var list = _containerSystem.EmptyContainer(component.Pit);

            foreach (var pitUid in list)
            {
                RemComp<StunnedComponent>(pitUid);
                _stun.TryKnockdown(pitUid, TimeSpan.FromSeconds(2), false);
            }
        }
        RemoveAllBingleGhosroles(uid, component);
    }
    public void RemoveAllBingleGhosroles(EntityUid uid, BinglePitComponent component)
    {
        var query = EntityQueryEnumerator<GhostRoleMobSpawnerComponent>();

        while (query.MoveNext(out var queryGRMSUid, out var queryGRMScomp))
        {
            if (queryGRMScomp.Prototype == "MobBingle")
                if (Transform(uid).Coordinates == Transform(queryGRMSUid).Coordinates)
                    QueueDel(queryGRMSUid); // remove any unspawned bingle when pit is destroyed
        }
    }
    private void OnAttacked(EntityUid uid, BinglePitComponent component, AttackedEvent args)
    {
        if (_containerSystem.ContainsEntity(uid, args.User))
            EnsureComp<StunnedComponent>(args.User);
    }
       private void OnUpdateCanMove(EntityUid uid, BinglePitFallingComponent component, UpdateCanMoveEvent args)
    {
        args.Cancel();
    }
}
