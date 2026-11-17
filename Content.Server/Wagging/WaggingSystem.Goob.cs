using Content.Shared._Shitmed.Humanoid.Events;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Wagging;
using System.Diagnostics.CodeAnalysis;

namespace Content.Server.Wagging;

public sealed partial class WaggingSystem
{
    [Dependency] private readonly MarkingManager _markingManager = default!;

    public bool TryGetWaggingMarking(string id, [NotNullWhen(true)] out string? wagging)
    {
        return _markingManager.TryGetWaggingMarking(id, out wagging);
    }

    public bool TryGetStillMarking(string id, [NotNullWhen(true)] out string? still)
    {
        return _markingManager.TryGetStillMarking(id, out still);
    }

    private void OnMarkingsChanged(Entity<WaggingComponent> ent, ref HumanoidMarkingsChangedEvent args)
    {
        RefreshWaggingAction(ent);
    }

    private void OnProfileLoadFinished(EntityUid uid, WaggingComponent component, ProfileLoadFinishedEvent args)
    {
        RefreshWaggingAction((uid, component));
    }

    public void RefreshWaggingAction(Entity<WaggingComponent> ent)
    {
        if (LifeStage(ent) < EntityLifeStage.MapInitialized || TerminatingOrDeleted(ent))
            return;

        if (HasWaggableTail(ent, out var wagging))
        {
            ent.Comp.Wagging = wagging;
            if (ent.Comp.ActionEntity == null)
                _actions.AddAction(ent, ref ent.Comp.ActionEntity, ent.Comp.Action, ent);
            return;
        }

        ent.Comp.Wagging = false;
        _actions.RemoveAction(ent.Owner, ent.Comp.ActionEntity);
        ent.Comp.ActionEntity = null;
    }

    public bool HasWaggableTail(Entity<WaggingComponent> ent, out bool wagging)
    {
        wagging = false;
        if (!TryComp<HumanoidAppearanceComponent>(ent, out var humanoid)
            || !humanoid.MarkingSet.Markings.TryGetValue(MarkingCategories.Tail, out var markings))
            return false;

        var waggable = false;
        foreach (var marking in markings)
        {
            if (TryGetStillMarking(marking.MarkingId, out _))
                wagging = waggable = true;
            else if (TryGetWaggingMarking(marking.MarkingId, out _))
                waggable = true;
        }

        return waggable;
    }
}
