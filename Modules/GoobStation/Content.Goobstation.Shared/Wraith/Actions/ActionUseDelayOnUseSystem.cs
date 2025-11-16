using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Actions.Events;

namespace Content.Goobstation.Shared.Wraith.Actions;

/// <summary>
/// Increments/Decrements the use delay of an action
/// </summary>
public sealed class ActionUseDelayOnUseSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    private EntityQuery<ActionComponent> _actionQuery;

    public override void Initialize()
    {
        base.Initialize();

        _actionQuery = GetEntityQuery<ActionComponent>();

        SubscribeLocalEvent<ActionUseDelayOnUseComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ActionUseDelayOnUseComponent, ActionPerformedEvent>(OnActionPerformed);
    }

    private void OnMapInit(Entity<ActionUseDelayOnUseComponent> ent, ref MapInitEvent args)
    {
        if (!_actionQuery.TryComp(ent, out var action)
            || action.UseDelay == null)
            return;

        ent.Comp.OriginalUseDelay = action.UseDelay.Value;
    }
    private void OnActionPerformed(Entity<ActionUseDelayOnUseComponent> ent, ref ActionPerformedEvent args)
    {
        if (!_actionQuery.TryComp(ent, out var action))
            return;

        var newUseDelay = action.UseDelay + ent.Comp.UseDelayAccumulator;
        _actions.SetUseDelay(ent.Owner, newUseDelay);
    }
}
