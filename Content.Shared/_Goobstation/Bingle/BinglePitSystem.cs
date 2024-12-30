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

namespace Content.Shared._Goobstation.Bingle;

public sealed class BinglePitSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem ContainerSystem = default!;
    [Dependency] private readonly BingleSystem _BingleSystem = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BinglePitComponent, StepTriggeredOffEvent>(OnStepTriggered);
        SubscribeLocalEvent<BinglePitComponent, StepTriggerAttemptEvent>(OnStepTriggerAttempt);
        SubscribeLocalEvent<BinglePitComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<BinglePitComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<ChasmFallingComponent, UpdateCanMoveEvent>(OnUpdateCanMove);
    }
    private void OnMapInit(Entity<BinglePitComponent> ent, ref MapInitEvent args)
    {
        //SpawnBingle(); //spawns a bingle ghostrole on startup
    }

    private void OnStepTriggered(EntityUid uid, BinglePitComponent component, ref StepTriggeredOffEvent args)
    {
        // dont add already falling and bingles
        if (HasComp<ChasmFallingComponent>(args.Tripper) || HasComp<BingleComponent>(args.Tripper))
            return;
        // after consuming start consuming living,
        if (HasComp<MobStateComponent>(args.Tripper) && !component.SwallowMobs)
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
        var falling = AddComp<ChasmFallingComponent>(tripper);

        if (HasComp<MobStateComponent>(tripper))
            component.Fallen = component.Fallen + 10;
        else
            component.Fallen++;

        ContainerSystem.Insert(tripper, component.Pit);

        //falling.NextDeletionTime = _timing.CurTime + falling.DeletionTime;
        //_blocker.UpdateCanMove(tripper);
        /*
        if (playSound)
            _audio.PlayPredicted(component.FallingSound, uid, tripper);*/
    }

    private void OnStepTriggerAttempt(EntityUid uid, BinglePitComponent component, ref StepTriggerAttemptEvent args)
    {
        args.Continue = true;
    }

    private void OnUpdateCanMove(EntityUid uid, ChasmFallingComponent component, UpdateCanMoveEvent args)
    {
        args.Cancel();
    }
    public void SpawnBingle(EntityUid uid, BinglePitComponent component)
    {
        //spawn normale bingle
    }
    public void UpgradeBingles(EntityUid uid, BinglePitComponent component)
    {
        var query = EntityQueryEnumerator<BingleComponent>();
        while (query.MoveNext(out var _uid, out var bingle))
        {
            _BingleSystem.UpgradeBingle(_uid, bingle);
        }
    }
    private void OnShutdown(EntityUid uid, BinglePitComponent component, ComponentShutdown args)
    {
        //eject inventory
    }
}
