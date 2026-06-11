// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Heretic.Prototypes;
using Content.Shared.Heretic;
using Content.Shared.Mind;
using Robust.Shared.Prototypes;

namespace Content.Server.Heretic.EntitySystems;

public sealed partial class HereticSystem
{
    public HereticKnowledgePrototype GetKnowledge(ProtoId<HereticKnowledgePrototype> id)
        => _proto.Index(id);

    public void RaiseKnowledgeEvent(EntityUid uid, HereticKnowledgeEvent ev, bool negative)
    {
        if (negative)
            EntityManager.RemoveComponents(uid, ev.AddedComponents);
        else
            EntityManager.AddComponents(uid, ev.AddedComponents);
        ev.Negative = negative;
        ev.Heretic = uid;
        RaiseLocalEvent(uid, (object) ev, true);
    }

    public bool TryAddKnowledge(Entity<HereticComponent?> ent,
        ProtoId<HereticKnowledgePrototype> id,
        EntityUid? body = null)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        body ??= CompOrNull<MindComponent>(ent.Owner)?.CurrentEntity;

        var data = GetKnowledge(id);

        if (data.Event != null && body != null)
        {
            RaiseKnowledgeEvent(body.Value, data.Event, false);
            ent.Comp.KnowledgeEvents.Add(data.Event);
        }

        if (data.ActionPrototypes is { Count: > 0 })
        {
            foreach (var act in data.ActionPrototypes)
            {
                _actionContainer.AddAction(ent.Owner, act);
            }
        }

        if (data.RitualPrototypes is { Count: > 0 })
        {
            foreach (var ritual in data.RitualPrototypes)
            {
                ent.Comp.KnownRituals.Add(_ritual.GetRitual(ritual));
            }
        }

        Dirty(ent);

        // set path if out heretic doesn't have it, or if it's different from whatever he has atm
        if (string.IsNullOrWhiteSpace(ent.Comp.CurrentPath))
        {
            if (!data.SideKnowledge && ent.Comp.CurrentPath != data.Path)
                ent.Comp.CurrentPath = data.Path;
        }

        // make sure we only progress when buying current path knowledge
        if (data.Stage > ent.Comp.PathStage && data.Path == ent.Comp.CurrentPath)
            ent.Comp.PathStage = data.Stage;

        return true;
    }
}
