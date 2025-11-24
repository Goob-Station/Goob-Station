using Content.Shared.Polymorph.Components;

namespace Content.Shared.Polymorph.Systems;

public abstract partial class SharedChameleonProjectorSystem
{
    /// <summary>
    /// Handles removing actions when the disguise component is shut down.
    /// </summary>
    // This is used for the Morph Antag
    private void OnDisguiseShutdown(Entity<ChameleonDisguiseComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.RemoveActions)
            _actions.RemoveProvidedActions(ent.Comp.User, ent.Comp.Projector);
        else
        {
            if (!TryComp<ChameleonProjectorComponent>(ent.Comp.Projector, out var comp))
                return;

            _actions.RemoveAction(comp.AnchorActionEntity);
            _actions.RemoveAction(comp.NoRotActionEntity);
        }
    }
}
