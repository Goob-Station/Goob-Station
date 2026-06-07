// SPDX-FileCopyrightText...
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Numerics;
using Content.Goobstation.Shared.Training;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Buckle.Components;
using Content.Shared.Tag;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Vehicles;

public sealed class ForkliftSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly SharedActionsSystem _action = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private const string CrateContainerId = "crate_storage";
    private static readonly ProtoId<TagPrototype> CrateTag = "Crate";
    private static readonly EntProtoId LiftForkActionId = "ActionForklift";
    private static readonly EntProtoId UnliftForkActionId = "ActionUnforklift";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ForkliftComponent, ComponentInit>(OnUpdate);
        SubscribeLocalEvent<ForkliftComponent, EntInsertedIntoContainerMessage>(OnUpdate);
        SubscribeLocalEvent<ForkliftComponent, EntRemovedFromContainerMessage>(OnUpdate);
        SubscribeLocalEvent<ForkliftComponent, UnstrappedEvent>(OnUnstrapped);
        SubscribeLocalEvent<ForkliftComponent, StrapAttemptEvent>(OnStrapAttempt);
        SubscribeLocalEvent<ForkliftActionEvent>(OnLiftForks);
        SubscribeLocalEvent<ForkliftComponent, UnforkliftActionEvent>(OnUnliftForks);

    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ForkliftComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.LiftSoundEndTime == null || _timing.CurTime < comp.LiftSoundEndTime.Value)
                continue;
            if (comp.LiftSoundUid != null)
            {
                _audio.Stop(comp.LiftSoundUid.Value);
                comp.LiftSoundUid = null;
            }
            comp.LiftSoundEndTime = null;
        }
    }

    private void OnUnliftForks(Entity<ForkliftComponent> ent, ref UnforkliftActionEvent args)
    {
        if (args.Handled || !_container.TryGetContainer(ent.Owner, CrateContainerId, out var container) || container.ContainedEntities.Count == 0)
            return;

        var targetCoords = Transform(ent).Coordinates.Offset(Transform(ent).LocalRotation.GetDir().ToVec());
        var crateToUnload = container.ContainedEntities.First();
        PlayForkliftSound(args.Performer, args.Action, ent);
        if (!_container.Remove(crateToUnload, container, destination: targetCoords))
            return;
        args.Handled = true;
    }

    private void OnLiftForks(ForkliftActionEvent args)
    {
        if (args.Handled
            || !TryComp<ForkliftComponent>(args.Action.Comp.Container, out var forkliftComp)
            || !_container.TryGetContainer(args.Action.Comp.Container.Value, CrateContainerId, out var container))
            return;

        var forkliftUid = args.Action.Comp.Container.Value;

        var capacity = HasComp<QuartermasterTrainingComponent>(args.Performer)
            ? forkliftComp.ForkliftCapacity * 2
            : forkliftComp.ForkliftCapacity;

        if (container.ContainedEntities.Count >= capacity || !_tag.HasTag(args.Target, CrateTag))
            return;
        _container.Insert(args.Target, container);
        args.Handled = true;

        PlayForkliftSound(args.Performer, args.Action, (forkliftUid, forkliftComp));
    }

    private void PlayForkliftSound(EntityUid driver, Entity<ActionComponent> action, Entity<ForkliftComponent> forklift)
    {
        if (forklift.Comp.LiftSoundUid != null)
            return;
        var audioEnt = _audio.PlayPredicted(forklift.Comp.LiftSound, forklift, driver, forklift.Comp.LiftSound.Params);
        if (!audioEnt.HasValue || action.Comp.UseDelay == null)
            return;
        forklift.Comp.LiftSoundUid = audioEnt.Value.Entity;
        forklift.Comp.LiftSoundEndTime = _timing.CurTime + action.Comp.UseDelay.Value;
    }

    private void OnUnstrapped(Entity<ForkliftComponent> ent, ref UnstrappedEvent args)
    {
        if (ent.Comp.LiftAction == null)
            return;

        _action.RemoveAction(args.Buckle.Owner, ent.Comp.LiftAction);
        _action.RemoveAction(args.Buckle.Owner, ent.Comp.UnliftAction);
    }

    private void OnStrapAttempt(Entity<ForkliftComponent> ent, ref StrapAttemptEvent args)
    {
        _action.AddAction(args.Buckle.Owner, ref ent.Comp.LiftAction, LiftForkActionId, ent);
        _action.AddAction(args.Buckle.Owner, ref ent.Comp.UnliftAction, UnliftForkActionId, ent);
    }

    private void OnUpdate<T>(Entity<ForkliftComponent> ent, ref T args)
    {
        UpdateAppearance(ent);
    }

    private void UpdateAppearance(Entity<ForkliftComponent> ent)
    {

        if(!_container.TryGetContainer(ent, CrateContainerId, out var container) || !TryComp<VehicleComponent>(ent, out var vehicle) || vehicle.ActiveOverlay == null)
            return;

        var state = container.ContainedEntities.Count switch
        {
            0 => ForkliftCrateState.Empty,
            1 => ForkliftCrateState.OneCrate,
            2 => ForkliftCrateState.TwoCrates,
            3 => ForkliftCrateState.ThreeCrates,
            _ => ForkliftCrateState.FourCrates,
        };

        _appearance.SetData(vehicle.ActiveOverlay.Value, ForkliftVisuals.CrateState, state);
    }
}
