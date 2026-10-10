using Content.Shared.Anomaly.Components;
using Robust.Shared.Prototypes;

namespace Content.Server.Anomaly.Effects;

public sealed partial class InnerBodyAnomalySystem
{
    private void GoobAddInjectedComponents(Entity<InnerBodyAnomalyComponent> ent, ComponentRegistry components)
    {
        var added = new ComponentRegistry();
        foreach (var (name, entry) in components)
        {
            if (!HasComp(ent, entry.Component.GetType()))
                added.Add(name, entry);
        }

        ent.Comp.AddedComponents = added;
        EntityManager.AddComponents(ent, added);
    }

    private void GoobRemoveInjectedComponents(Entity<InnerBodyAnomalyComponent> ent)
    {
        if (ent.Comp.AddedComponents is not { } added)
            return;

        EntityManager.RemoveComponents(ent, added);
        ent.Comp.AddedComponents = null;
    }
}
