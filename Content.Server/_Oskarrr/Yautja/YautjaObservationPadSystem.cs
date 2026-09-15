// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Content.Shared._Oskarrr.Yautja.Components;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._Oskarrr.Yautja;

/// <summary>
/// Observation pad projection without DV Psionics mind-swap (not present in Badlands).
/// Uses mind TransferTo + return action, same pattern as Cosmic astral projection.
/// </summary>
public sealed class YautjaObservationPadSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<YautjaObservationPadComponent, YautjaObservationProjectionEvent>(OnProjection);
        SubscribeLocalEvent<YautjaObservationProjectionComponent, YautjaObservationReturnEvent>(OnReturn);
    }

    private void OnProjection(
        Entity<YautjaObservationPadComponent> ent,
        ref YautjaObservationProjectionEvent args)
    {
        if (args.Handled || !TryComp<MindContainerComponent>(args.Performer, out _))
            return;

        if (!_mind.TryGetMind(args.Performer, out var mindId, out _))
            return;

        var projection = Spawn(ent.Comp.ProjectionPrototype, Transform(args.Performer).Coordinates);
        _transform.AttachToGridOrMap(projection);

        var projComp = EnsureComp<YautjaObservationProjectionComponent>(projection);
        projComp.OriginalBody = args.Performer;
        Dirty(projection, projComp);

        _mind.TransferTo(mindId, projection);

        EntityUid? returnAction = null;
        _actions.AddAction(projection, ref returnAction, ent.Comp.ReturnActionPrototype);
        if (returnAction is { } action)
        {
            _actions.SetUseDelay(action, ent.Comp.ReturnDelay);
            _actions.StartUseDelay(action);
        }

        args.Handled = true;
    }

    private void OnReturn(
        Entity<YautjaObservationProjectionComponent> ent,
        ref YautjaObservationReturnEvent args)
    {
        if (args.Handled)
            return;

        if (!Exists(ent.Comp.OriginalBody))
        {
            QueueDel(ent.Owner);
            args.Handled = true;
            return;
        }

        if (_mind.TryGetMind(args.Performer, out var mindId, out _))
            _mind.TransferTo(mindId, ent.Comp.OriginalBody);

        QueueDel(ent.Owner);
        args.Handled = true;
    }
}
