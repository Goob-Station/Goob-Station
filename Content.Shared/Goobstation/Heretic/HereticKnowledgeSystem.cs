using Content.Shared.Actions;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;

namespace Content.Shared.Heretic;

public sealed partial class HereticKnowledgeSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedActionsSystem _action = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public HereticKnowledgePrototype GetKnowledge(ProtoId<HereticKnowledgePrototype> id)
        => (HereticKnowledgePrototype) _proto.Index(typeof(HereticKnowledgePrototype), id);

    public void AddKnowledge(Entity<HereticComponent> ent, ProtoId<HereticKnowledgePrototype> id)
    {
        var data = GetKnowledge(id);

        if (data.Event != null)
            RaiseLocalEvent(data.Event);

        if (data.ActionPrototype != null)
            _action.AddAction(ent, data.ActionPrototype);

        if (data.RitualPrototype != null)
            ent.Comp.KnownRituals.Add(data.RitualPrototype.ID);

        _popup.PopupEntity(Loc.GetString("heretic-knowledge-gain"), ent, ent);
    }
    public void RemoveKnowledge(Entity<HereticComponent> ent, ProtoId<HereticKnowledgePrototype> id)
    {
        var data = GetKnowledge(id);

        if (data.Event != null)
            RaiseLocalEvent(data.Event);

        if (data.ActionPrototype != null)
        {
            var actionName = (EntityPrototype) _proto.Index(typeof(EntityPrototype), data.ActionPrototype);

            foreach (var action in _action.GetActions(ent))
            {
                // jesus christ.
                if (!TryComp<MetaDataComponent>(action.Id, out var metadata))
                    continue;

                if (metadata.EntityName == actionName.Name)
                    _action.RemoveAction(action.Id);
            }
        }

        if (data.RitualPrototype != null)
            ent.Comp.KnownRituals.Remove(data.RitualPrototype.ID);

        _popup.PopupEntity(Loc.GetString("heretic-knowledge-loss"), ent, ent);
    }
}
