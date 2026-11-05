using Content.Goobstation.Shared.MartialArts.Events;
using Content.Goobstation.Shared.MartialArts.Specific.Components;
using Content.Shared.Actions;

namespace Content.Goobstation.Shared.MartialArts.Specific;

public sealed partial class KravMagaSystem
{
    [Dependency] private SharedActionsSystem _actions = default!;

    [SubscribeLocalEvent]
    private void OnMapInit(Entity<KravMagaComponent> ent, ref MapInitEvent args)
    {
        var ev = new BeforeKravMagaAdded();
        RaiseLocalEvent(ent, ref ev);
        if (ev.Cancelled)
        {
            RemComp<KravMagaComponent>(ent);
            return;
        }

        foreach (var actionId in ent.Comp.BaseKravMagaMoves)
        {
            var actions = _actions.AddAction(ent, actionId);
            if (actions != null)
                ent.Comp.KravMagaMoveEntities.Add(actions.Value);
        }
    }

    [SubscribeLocalEvent]
    private void OnKravMagaShutdown(Entity<KravMagaComponent> ent, ref ComponentShutdown args)
    {
        foreach (var action in ent.Comp.KravMagaMoveEntities)
        {
            _actions.RemoveAction(action);
        }
    }
}
