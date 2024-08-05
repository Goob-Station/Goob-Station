using Content.Shared.Actions;
using Content.Shared.Heretic;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;
using System.Text;

namespace Content.Server.Heretic;

public sealed partial class HereticKnowledgeSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedActionsSystem _action = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly HereticRitualSystem _ritual = default!;

    public HereticKnowledgePrototype GetKnowledge(ProtoId<HereticKnowledgePrototype> id)
        => _proto.Index(id);

    private void AddKnowledge(Entity<HereticComponent> ent, string listingEventId)
    {
        // basically remove the Listing and the Event, leave only the name
        var sb = new StringBuilder();
        sb.Append(listingEventId);
        sb.Remove(0, 7);
        sb.Remove(listingEventId.Length - 6, 5);

        _popup.PopupEntity(sb.ToString(), ent, ent);
        AddKnowledge(ent, ent.Comp, sb.ToString());
    }
    public void AddKnowledge(EntityUid uid, HereticComponent comp, ProtoId<HereticKnowledgePrototype> id, bool silent = true)
    {
        var data = GetKnowledge(id);

        if (data.Event != null)
            RaiseLocalEvent(data.Event);

        if (data.ActionPrototypes != null && data.ActionPrototypes.Count > 0)
            foreach (var act in data.ActionPrototypes)
                _action.AddAction(uid, act);

        if (data.RitualPrototypes != null && data.RitualPrototypes.Count > 0)
            foreach (var ritual in data.RitualPrototypes)
                comp.KnownRituals.Add(_ritual.GetRitual(ritual));

        if (data.Stage > comp.PathStage)
            comp.PathStage = data.Stage;

        if (!silent)
            _popup.PopupEntity(Loc.GetString("heretic-knowledge-gain"), uid, uid);
    }
    public void RemoveKnowledge(EntityUid uid, HereticComponent comp, ProtoId<HereticKnowledgePrototype> id, bool silent = false)
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
                foreach (var action in _action.GetActions(uid))
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
                comp.KnownRituals.Remove(_ritual.GetRitual(ritual));

        if (!silent)
            _popup.PopupEntity(Loc.GetString("heretic-knowledge-loss"), uid, uid);
    }

    public override void Initialize()
    {
        base.Initialize();

        // i couldn't find a better way, e.g. getting the listing prototype, because it simply doesn't get passed
        // so i'm using a shit ton of events. god help me tolerate this. this is a lot of copypaste.
        SubscribeLocalEvent<HereticComponent, ListingNightwatcherSecretEvent>(ListingNightwatcherSecret);
    }

    private void ListingNightwatcherSecret(Entity<HereticComponent> ent, ref ListingNightwatcherSecretEvent args)
    => AddKnowledge(ent, nameof(args));
}
