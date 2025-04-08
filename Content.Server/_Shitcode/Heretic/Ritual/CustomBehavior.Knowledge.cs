// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 TGRCDev <tgrc@tgrc.dev>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 MJSailor <92106367+kurokoTurbo@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Heretic.EntitySystems;
using Content.Shared.Dataset;
using Content.Shared.Heretic;
using Content.Shared.Heretic.Prototypes;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System.Text;
using Robust.Server.Containers;

namespace Content.Server.Heretic.Ritual;

public sealed partial class RitualKnowledgeBehavior : RitualCustomBehavior
{
    private HashSet<ProtoId<TagPrototype>> _missingTags = new();
    private List<EntityUid> _toDelete = new();

    private IPrototypeManager _prot = default!;
    private IRobustRandom _rand = default!;
    private EntityLookupSystem _lookup = default!;
    private HereticSystem _heretic = default!;
    private TagSystem _tag = default!;
    private ContainerSystem _container = default!;

    // this is basically a ripoff from hereticritualsystem
    public override bool Execute(RitualData args, out string? outstr)
    {
        _prot = IoCManager.Resolve<IPrototypeManager>();
        _rand = IoCManager.Resolve<IRobustRandom>();
        _lookup = args.EntityManager.System<EntityLookupSystem>();
        _heretic = args.EntityManager.System<HereticSystem>();
        _tag = args.EntityManager.System<TagSystem>();
        _container = args.EntityManager.System<ContainerSystem>();

        outstr = null;

        if (!args.EntityManager.TryGetComponent(args.Performer, out HereticComponent? heretic))
            return false;

        var requiredTags = _heretic.TryGetRequiredKnowledgeTags((args.Performer, heretic));

        if (requiredTags == null)
            return false;

        var lookup = _lookup.GetEntitiesInRange(args.Platform, 1.5f);

        _toDelete.Clear();
        _missingTags.Clear();
        _missingTags.UnionWith(requiredTags);
        foreach (var look in lookup)
        {
            if (!args.EntityManager.TryGetComponent<TagComponent>(look, out var tags))
                continue;

            if (_container.IsEntityInContainer(look))
                continue;

            _missingTags.RemoveWhere(tag =>
            {
                if (_tag.HasTag(tags, tag))
                {
                    _toDelete.Add(look);
                    return true;
                }

                return false;
            });
        }

        if (_missingTags.Count > 0)
        {
            var missing = string.Join(", ", _missingTags);
            outstr = Loc.GetString("heretic-ritual-fail-items", ("itemlist", missing));
            return false;
        }

        return true;
    }

    public override void Finalize(RitualData args)
    {
        // delete all and reset
        foreach (var ent in _toDelete)
            args.EntityManager.QueueDeleteEntity(ent);
        _toDelete.Clear();

        if (!args.EntityManager.TryGetComponent<HereticComponent>(args.Performer, out var hereticComp))
            return;

        _heretic.UpdateKnowledge(args.Performer, hereticComp, 4);
        _heretic.GenerateRequiredKnowledgeTags((args.Performer, hereticComp));
    }
}