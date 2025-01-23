using Content.Shared.Ghost.Roles.Components;
using Content.Shared.StepTrigger.Systems;
using Content.Shared.Mobs.Components;
using Robust.Shared.Containers;
using Content.Shared.Destructible;
using Content.Shared._Goobstation.Bingle;
using Content.Shared.Stunnable;
using Content.Server.Stunnable;
using Content.Shared.Humanoid;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Audio.Systems;
using Content.Shared.Weapons.Melee.Events;
namespace Content.Server._Goobstation.Bingle;

public sealed class BinglePitSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly BingleSystem _bingleSystem = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly FixtureSystem _fixtures = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BinglePitComponent, StepTriggeredOffEvent>(OnStepTriggered);
        SubscribeLocalEvent<BinglePitComponent, StepTriggerAttemptEvent>(OnStepTriggerAttempt);
        SubscribeLocalEvent<BinglePitComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<BinglePitComponent, DestructionEventArgs>(OnDestruction);
        SubscribeLocalEvent<BinglePitComponent, AttackedEvent>(OnAttacked);
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
        // need to be at levl 1 or above to swallo anything alive
        if (HasComp<MobStateComponent>(args.Tripper) && component.Level == 0)
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

        _containerSystem.Insert(tripper, component.Pit);
        EnsureComp<StunnedComponent>(tripper); // used stuned to prevent any funny being done inside the pit

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
                //todo:add knock down
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
            //TODO: add a range check
            if (queryGRMScomp.Prototype == "MobBingle")
                QueueDel(queryGRMSUid);
        }
    }
    private void OnAttacked(EntityUid uid, BinglePitComponent component, AttackedEvent args)
    {
        if (_containerSystem.ContainsEntity(uid, args.User))
            EnsureComp<StunnedComponent>(args.User);
    }
}
