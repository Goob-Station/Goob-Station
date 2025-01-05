using Content.Shared.Ghost.Roles.Components;
using Content.Shared.StepTrigger.Systems;
using Content.Shared.Mobs.Components;
using Robust.Shared.Containers;
using Content.Shared.Destructible;
using Content.Shared._Goobstation.Bingle;
using Content.Shared.Stunnable;
using Content.Shared.Humanoid;

namespace Content.Server._Goobstation.Bingle;

public sealed class BinglePitSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly BingleSystem _bingleSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BinglePitComponent, StepTriggeredOffEvent>(OnStepTriggered);
        SubscribeLocalEvent<BinglePitComponent, StepTriggerAttemptEvent>(OnStepTriggerAttempt);
        SubscribeLocalEvent<BinglePitComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<BinglePitComponent, DestructionEventArgs>(OnDestruction);
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

    public void StartFalling(EntityUid uid, BinglePitComponent component, EntityUid tripper)
    {
        if (HasComp<MobStateComponent>(tripper))
        {
            component.BinglePoints = component.BinglePoints + 5f;
            if (HasComp<HumanoidAppearanceComponent>(tripper))
                component.BinglePoints = component.BinglePoints + 5f;
        }
        else
            component.BinglePoints++;

        if (component.Pit == null)
            component.Pit = _containerSystem.EnsureContainer<Container>(uid, "pit");

        _containerSystem.Insert(tripper, component.Pit);
        EnsureComp<StunnedComponent>(tripper);
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
        while (query.MoveNext(out var _uid, out var bingle))
        {
            _bingleSystem.UpgradeBingle(_uid, bingle);
        }
    }
    private void OnDestruction(EntityUid uid, BinglePitComponent component, DestructionEventArgs args)
    {
        if (component.Pit != null)
        {
            var list = _containerSystem.EmptyContainer(component.Pit);

            foreach (var pitUid in list)
            {
                RemComp<StunnedComponent>(pitUid);
            }
        }
        RemoveAllBingleGhosroles();
    }
    public void RemoveAllBingleGhosroles()
    {
        var query = EntityQueryEnumerator<GhostRoleMobSpawnerComponent>();

        while (query.MoveNext(out var GRMSUid, out var GRMScomp))
        {
            //TODO: add a range check
            if (GRMScomp.Prototype == "MobBingle")
                QueueDel(GRMSUid);
        }
    }
}
