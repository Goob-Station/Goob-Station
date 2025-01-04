using Content.Shared.ActionBlocker;
using Content.Shared.Buckle.Components;
using Content.Shared.Movement.Events;
using Content.Shared.StepTrigger.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Physics.Components;
using Robust.Shared.Timing;
using Content.Shared.Chasm;
using Content.Shared.Mobs.Components;
using Robust.Shared.Containers;
using Content.Shared.Destructible;
using Robust.Shared.Network;
using Content.Shared._Goobstation.Bingle;
using Content.Shared.Stunnable;

namespace Content.Server._Goobstation.Bingle;

public sealed class BinglePitSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem ContainerSystem = default!;
    [Dependency] private readonly BingleSystem _BingleSystem = default!;

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
        component.Pit = ContainerSystem.EnsureContainer<Container>(uid, "pit");
    }
    private void OnStepTriggered(EntityUid uid, BinglePitComponent component, ref StepTriggeredOffEvent args)
    {
        // dont add already falling and bingles
        if (HasComp<BingleComponent>(args.Tripper))
            return;
        // after consuming start consuming living,
        if (HasComp<MobStateComponent>(args.Tripper) && (component.Level==0))
            return;

        StartFalling(uid, component, args.Tripper);

        if (component.Fallen >= component.SpawnNewAt)
        {
            SpawnBingle(uid, component);
            component.Fallen = component.Fallen - component.SpawnNewAt;
        }
    }

    public void StartFalling(EntityUid uid, BinglePitComponent component, EntityUid tripper, bool playSound = true)
    {
        if (HasComp<MobStateComponent>(tripper))
            component.Fallen = component.Fallen + 10f;
        else
            component.Fallen++;

        if (component.Pit == null)
            component.Pit = ContainerSystem.EnsureContainer<Container>(uid, "pit");

        ContainerSystem.Insert(tripper, component.Pit);
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
            UpgradeBingles(uid,component);
    }
    public void UpgradeBingles(EntityUid uid, BinglePitComponent component)
    {
        var query = EntityQueryEnumerator<BingleComponent>();
        while (query.MoveNext(out var _uid, out var bingle))
        {
           _BingleSystem.UpgradeBingle(_uid, bingle);
        }
    }
    private void OnDestruction(EntityUid uid, BinglePitComponent component, DestructionEventArgs args)
    {
        if (component.Pit != null)
        {
            var list =  ContainerSystem.EmptyContainer(component.Pit);

            foreach (var pitUid in list)
            {
                RemComp<StunnedComponent>(pitUid);
            }
        }
    }
}
