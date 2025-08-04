// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Server.Heretic.Components;
using Content.Goobstation.Server.Heretic.EntitySystems;
using Content.Goobstation.Shared.Heretic.Components;
using Content.Goobstation.Shared.Heretic.Prototypes;
using Content.Server._Goobstation.Objectives.Components;
using Content.Server.Body.Systems;
using Content.Shared.Mobs.Components;
using Content.Shared.Humanoid;
using Content.Server.Revolutionary.Components;
using Content.Shared.Mind;
using Content.Shared.Gibbing.Events;
using Content.Shared.Mobs.Systems;

namespace Content.Goobstation.Server.Heretic.Ritual;

/// <summary>
///     Checks for the nearest dead body,
///     gibs it and gives the heretic knowledge points.
/// </summary>
// these classes should be lead out and shot
[Virtual]
public partial class RitualSacrificeBehavior : RitualCustomBehavior
{
    /// <summary>
    ///     Minimal amount of corpses.
    /// </summary>
    [DataField]
    public float Min = 1;

    /// <summary>
    ///     Maximum amount of corpses.
    /// </summary>
    [DataField]
    public float Max = 1;

    /// <summary>
    ///     Should we count only targets?
    /// </summary>
    [DataField]
    public bool OnlyTargets;

    // this is awful but it works so i'm not complaining
    private SharedMindSystem _mind = default!;
    private HereticSystem _heretic = default!;
    private BodySystem _body = default!;
    private EntityLookupSystem _lookup = default!;
    private MobStateSystem _mobState = default!;

    protected List<EntityUid> Entities = [];

    public override bool Execute(RitualData args, out string? outstr)
    {
        _mind = args.EntityManager.System<SharedMindSystem>();
        _heretic = args.EntityManager.System<HereticSystem>();
        _body = args.EntityManager.System<BodySystem>();
        _lookup = args.EntityManager.System<EntityLookupSystem>();
        _mobState = args.EntityManager.System<MobStateSystem>();

        Entities = [];

        if (!args.EntityManager.TryGetComponent<HereticComponent>(args.Performer, out var hereticComp))
        {
            outstr = string.Empty;
            return false;
        }

        var lookup = _lookup.GetEntitiesInRange(args.Platform, 1.5f);
        if (lookup.Count == 0)
        {
            outstr = Loc.GetString("heretic-ritual-fail-sacrifice");
            return false;
        }

        // get all the dead ones
        foreach (var ent in lookup)
        {
            if (!args.EntityManager.TryGetComponent<MobStateComponent>(ent, out var mobstate) // Only mobs.
            || !args.EntityManager.HasComponent<HumanoidAppearanceComponent>(ent) // Only humans.
            || OnlyTargets && hereticComp.SacrificeTargets.All(x => x.Entity != args.EntityManager.GetNetEntity(ent))) // Only targets.
                continue;

            if (_mobState.IsDead(ent))
                Entities.Add(ent);
        }

        if (Entities.Count < Min)
        {
            outstr = Loc.GetString("heretic-ritual-fail-sacrifice-ineligible");
            return false;
        }

        outstr = null;
        return true;
    }

    public override void Finalize(RitualData args)
    {
        if (!args.EntityManager.TryGetComponent(args.Performer, out HereticComponent? heretic))
        {
            Entities = [];
            return;
        }

        for (var i = 0; i < Max && i < Entities.Count; i++)
        {
            if (!args.EntityManager.EntityExists(Entities[i]))
                continue;

            var (isCommand, isSec) = IsCommandOrSec(Entities[i], args.EntityManager);
            var knowledgeGain = heretic.SacrificeTargets
                .Any(x => x.Entity == args.EntityManager.GetNetEntity(Entities[i]))
                ? isCommand || isSec ? 3f : 2f
                : 0f;

            // YES!!! GIB!!!
            _body.GibBody(Entities[i], contents: GibContentsOption.Gib);

            if (knowledgeGain > 0)
                _heretic.UpdateKnowledge(args.Performer, heretic, knowledgeGain);

            // update objectives
            if (!_mind.TryGetMind(args.Performer, out var mindId, out var mind))
                continue;

            // this is godawful dogshit. but it works :)
            if (_mind.TryFindObjective((mindId, mind), "HereticSacrificeObjective", out var crewObj)
                && args.EntityManager.TryGetComponent<HereticSacrificeConditionComponent>(crewObj, out var crewObjComp))
                crewObjComp.Sacrificed += 1;

            if (_mind.TryFindObjective((mindId, mind), "HereticSacrificeHeadObjective", out var crewHeadObj)
                && args.EntityManager.TryGetComponent<HereticSacrificeConditionComponent>(crewHeadObj, out var crewHeadObjComp)
                && isCommand)
                crewHeadObjComp.Sacrificed += 1;
        }

        // reset it because it refuses to work otherwise.
        Entities.Clear();
        args.EntityManager.EventBus.RaiseLocalEvent(args.Performer, new EventHereticUpdateTargets());
    }

    protected static (bool isCommand, bool isSec) IsCommandOrSec(EntityUid uid, IEntityManager entityManager)
    {
        return (entityManager.HasComponent<CommandStaffComponent>(uid),
            entityManager.HasComponent<SecurityStaffComponent>(uid));
    }
}
