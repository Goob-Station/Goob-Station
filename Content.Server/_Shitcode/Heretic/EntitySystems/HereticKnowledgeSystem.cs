// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Content.Shared.Heretic.Prototypes;
using Content.Shared.Heretic;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;

namespace Content.Server.Heretic.EntitySystems;

public sealed partial class HereticKnowledgeSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedActionsSystem _action = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly HereticRitualSystem _ritual = default!;

    public HereticKnowledgePrototype GetKnowledge(ProtoId<HereticKnowledgePrototype> id)
        => _proto.Index(id);

    public void AddKnowledge(EntityUid uid, HereticComponent comp, ProtoId<HereticKnowledgePrototype> id, bool silent = true, bool research = true)
    {
        var data = GetKnowledge(id);

        if (data.Event != null)
            RaiseLocalEvent(uid, data.Event, true);

        if (data.ActionPrototypes != null && data.ActionPrototypes.Count > 0)
        {
            foreach (var act in data.ActionPrototypes)
            {
                if (_action.AddAction(uid, act) is {} action)
                    comp.ProvidedActions.Add(action);
                else
                    Log.Error($"Failed to give heretic {ToPrettyString(uid)} action {act}!");
            }
        }

        if (data.RitualPrototypes != null && data.RitualPrototypes.Count > 0)
            foreach (var ritual in data.RitualPrototypes)
                comp.KnownRituals.Add(_ritual.GetRitual(ritual));

        Dirty(uid, comp);

        // set path if out heretic doesn't have it, or if it's different from whatever he has atm
        if (string.IsNullOrWhiteSpace(comp.CurrentPath))
        {
            if (!data.SideKnowledge && comp.CurrentPath != data.Path)
                comp.CurrentPath = data.Path;
        }

        // make sure we only progress when buying current path knowledge
        if (data.Stage > comp.PathStage && data.Path == comp.CurrentPath)
            comp.PathStage = data.Stage;

        if (!silent) _popup.PopupEntity(Loc.GetString("heretic-knowledge-gain"), uid, uid);
        if (research) comp.ResearchedKnowledge.Add(id);
    }
    public void RemoveKnowledge(EntityUid uid, HereticComponent comp, ProtoId<HereticKnowledgePrototype> id, bool silent = false)
    {
        var data = GetKnowledge(id);

        if (data.ActionPrototypes != null && data.ActionPrototypes.Count > 0)
        {
            foreach (var act in data.ActionPrototypes)
            {
                comp.ProvidedActions.RemoveAll(action =>
                {
                    // goida
                    if (Prototype(action)?.ID is not {} id || id != act)
                        return false;

                    _action.RemoveAction(action);
                    return true;
                });
            }
        }

        if (data.RitualPrototypes != null && data.RitualPrototypes.Count > 0)
            foreach (var ritual in data.RitualPrototypes)
                comp.KnownRituals.Remove(_ritual.GetRitual(ritual));

        Dirty(uid, comp);

        if (!silent)
            _popup.PopupEntity(Loc.GetString("heretic-knowledge-loss"), uid, uid);
    }
}
