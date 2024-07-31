using Content.Shared.Actions;
using Content.Shared.Heretic;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;

namespace Content.Server.Heretic;

public sealed partial class HereticKnowledgeSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedActionsSystem _action = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly HereticRitualSystem _ritual = default!;

    public HereticKnowledgePrototype GetKnowledge(ProtoId<HereticKnowledgePrototype> id)
        => _proto.Index(id);

    public void AddKnowledge(Entity<HereticComponent> ent, ProtoId<HereticKnowledgePrototype> id)
    {
        var data = GetKnowledge(id);

        if (data.Event != null)
            RaiseLocalEvent(data.Event);

        if (data.ActionPrototypes != null && data.ActionPrototypes.Count > 0)
            foreach (var act in data.ActionPrototypes)
                _action.AddAction(ent, act);

        if (data.RitualPrototypes != null && data.RitualPrototypes.Count > 0)
            foreach (var ritual in data.RitualPrototypes)
                ent.Comp.KnownRituals.Add(_ritual.GetRitual(ritual));

        _popup.PopupEntity(Loc.GetString("heretic-knowledge-gain"), ent, ent);
    }
    public void RemoveKnowledge(Entity<HereticComponent> ent, ProtoId<HereticKnowledgePrototype> id)
    {
        var data = GetKnowledge(id);

        if (data.Event != null)
            RaiseLocalEvent(data.Event);

        if (data.ActionPrototypes != null && data.ActionPrototypes.Count > 0)
        {
            foreach (var act in data.ActionPrototypes)
            {
                var actionName = (EntityPrototype) _proto.Index(typeof(EntityPrototype), act);
                // jesus christ.
                foreach (var action in _action.GetActions(ent))
                {
                    if (!TryComp<MetaDataComponent>(action.Id, out var metadata))
                        continue;
                    if (metadata.EntityName == actionName.Name)
                        _action.RemoveAction(action.Id);
                }
            }
        }

        if (data.RitualPrototypes != null && data.RitualPrototypes.Count > 0)
            foreach (var ritual in data.RitualPrototypes)
                ent.Comp.KnownRituals.Remove(_ritual.GetRitual(ritual));

        _popup.PopupEntity(Loc.GetString("heretic-knowledge-loss"), ent, ent);
    }
}
