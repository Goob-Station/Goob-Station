// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Common.Heretic;
using Content.Server._Goobstation.Objectives.Components;
using Content.Server.Body.Systems;
using Content.Server.Heretic.Components;
using Content.Shared.Heretic.Prototypes;
using Content.Shared.Mobs.Components;
using Robust.Shared.Prototypes;
using Content.Shared.Humanoid;
using Content.Server.Revolutionary.Components;
using Content.Shared.Body.Components; // Pirate
using Content.Shared.Body.Organ; // Pirate
using Content.Shared.Damage; // Pirate
using Content.Shared.Damage.Prototypes; // Pirate
using Content.Shared.Damage.Systems; // Pirate
using Content.Shared.Mind;
using Content.Shared.Mobs; // Pirate
using Content.Shared.Mobs.Systems; // Pirate
using Content.Shared.Heretic;
using Content.Server.Heretic.EntitySystems;
using Content.Shared.Gibbing.Events;
using Content.Shared.Silicons.Borgs.Components;
using Content.Goobstation.Shared.Teleportation.Systems; // Pirate
using Robust.Shared.Random; // Pirate
using Robust.Shared.GameObjects; // Pirate
using Content.Shared._Shitmed.Targeting; // Pirate
using Content.Shared.Store.Components;

namespace Content.Server.Heretic.Ritual;

/// <summary>
///     Checks for a nearest dead body,
///     gibs it and gives the heretic knowledge points.
/// </summary>
// these classes should be lead out and shot
[Virtual] public partial class RitualSacrificeBehavior : RitualCustomBehavior
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

    /// <summary>
    ///     Should we count only humanoids?
    /// </summary>
    [DataField]
    public bool OnlyHumanoid = true;

    // this is awful but it works so i'm not complaining
    protected SharedMindSystem _mind = default!;
    protected HereticSystem _heretic = default!;
    protected BodySystem _body = default!;
    protected EntityLookupSystem _lookup = default!;
    protected DamageableSystem _damageable = default!; // Pirate
    protected SharedRandomTeleportSystem _randomTeleport = default!; // Pirate
    protected MobStateSystem _mobState = default!; // Pirate
    [Dependency] protected IRobustRandom _random = default!; // Pirate
    [Dependency] protected IPrototypeManager _proto = default!;
    [Dependency] protected ILogManager _log = default!;

    private EntityQuery<MobStateComponent>? _mobStateQuery; // Pirate

    protected List<EntityUid> uids = new();

    public override bool Execute(RitualData args, out string? outstr)
    {
        _mind = args.EntityManager.System<SharedMindSystem>();
        _heretic = args.EntityManager.System<HereticSystem>();
        _body = args.EntityManager.System<BodySystem>();
        _lookup = args.EntityManager.System<EntityLookupSystem>();
        _damageable = args.EntityManager.System<DamageableSystem>(); // Pirate
        _randomTeleport = args.EntityManager.System<SharedRandomTeleportSystem>(); // Pirate
        _proto = IoCManager.Resolve<IPrototypeManager>();
        _log = IoCManager.Resolve<ILogManager>();
        _random = IoCManager.Resolve<IRobustRandom>(); // Pirate
        _mobState = args.EntityManager.System<MobStateSystem>(); // Pirate
        _mobStateQuery = args.EntityManager.GetEntityQuery<MobStateComponent>(); // Pirate

        uids = new();

        var hereticComp = args.Mind.Comp;

        var lookup = _lookup.GetEntitiesInRange(args.Platform, 1.5f);
        if (lookup.Count == 0)
        {
            outstr = Loc.GetString("heretic-ritual-fail-sacrifice");
            return false;
        }

        // get all the dead ones
        foreach (var look in lookup)
        {
            if (!args.EntityManager.TryGetComponent<MobStateComponent>(look, out var mobstate) // only mobs
            || OnlyHumanoid && !args.EntityManager.HasComponent<HumanoidAppearanceComponent>(look) // only humans
            || args.EntityManager.HasComponent<BorgChassisComponent>(look) // no borgs
            || OnlyTargets
                && hereticComp.SacrificeTargets.All(x => x.Entity != args.EntityManager.GetNetEntity(look)) // only targets
                && !_heretic.TryGetHereticComponent(look, out _, out _)) // or other heretics
                continue;

            if (mobstate.CurrentState != Shared.Mobs.MobState.Alive)
                uids.Add(look);
        }

        if (uids.Count < Min)
        {
            outstr = Loc.GetString("heretic-ritual-fail-sacrifice-ineligible");
            return false;
        }

        outstr = null;
        return true;
    }

    public override void Finalize(RitualData args)
    {
        _mobStateQuery ??= args.EntityManager.GetEntityQuery<MobStateComponent>(); // Pirate
        _damageable ??= args.EntityManager.System<DamageableSystem>(); // Pirate

        var heretic = args.Mind.Comp;

        if (!args.EntityManager.TryGetComponent(args.Mind, out StoreComponent? store) ||
            !args.EntityManager.TryGetComponent(args.Mind, out MindComponent? mind))
            return;

        var knowledgeGain = 0f;
        for (var i = 0; i < Max && i < uids.Count; i++)
        {
            if (!args.EntityManager.EntityExists(uids[i]))
                continue;

            var uid = uids[i];

            var isCommand = args.EntityManager.HasComponent<CommandStaffComponent>(uid);
            var isSec = args.EntityManager.HasComponent<SecurityStaffComponent>(uid);
            var isHeretic = _heretic.TryGetHereticComponent(uid, out var otherHeretic, out var otherMind);
            knowledgeGain +=
                isHeretic ||
                heretic.SacrificeTargets.Any(x => x.Entity == args.EntityManager.GetNetEntity(uid))
                    ? isCommand || isSec || isHeretic ? 3f : 2f
                    : 0f;

            // Pirate start
            if (!args.EntityManager.TryGetComponent<BodyComponent>(uid, out var body))
                continue;

            // Kill critical targets with massive asphyxiation damage
            if (_mobStateQuery.Value.TryComp(uid, out var mobState) && mobState.CurrentState == MobState.Critical)
            {
                var proto = _proto ?? IoCManager.Resolve<IPrototypeManager>();
                var asphyxiation = new DamageSpecifier(proto.Index<DamageTypePrototype>("Asphyxiation"), 1000f);
                var damageable = _damageable ?? args.EntityManager.System<DamageableSystem>();
                damageable.TryChangeDamage(uid, asphyxiation, ignoreResistances: true, interruptsDoAfters: true,
                    origin: args.Performer, targetPart: TargetBodyPart.All, ignoreBlockers: true);
            }

            // Get all organs from the body
            var allOrgans = _body.GetBodyOrgans(uid, body).ToList();
            // Filter out brain and kidneys
            var eligibleOrgans = allOrgans
                .Where(o => o.Component.SlotId != "brain" && o.Component.SlotId != "kidneys")
                .Select(o => o.Id)
                .ToList();

            // Select 1-3 random organs and drop them at ritual site
            if (_random == null)
            {
                continue;
            }
            var numToExtract = _random.Next(1, 4);
            var organsToExtract = eligibleOrgans
                .OrderBy(_ => _random.Next())
                .Take(Math.Min(numToExtract, eligibleOrgans.Count))
                .ToList();
            foreach (var organ in organsToExtract)
            {
                _body.RemoveOrgan(organ);
            }

            // Teleport the corpse to a random safe location on the station
            _randomTeleport.RandomTeleportToStation(uid, 50, false);

            // Remove sacrificed target from heretic target list
            heretic.SacrificeTargets.RemoveAll(x => x.Entity == args.EntityManager.GetNetEntity(uid));
            //Pirate end

            // Sacrificed heretics lose their powers forever
            if (otherMind != EntityUid.Invalid && otherHeretic is { } h)
                args.EntityManager.RemoveComponentDeferred(otherMind, h);

            // update objectives
            // this is godawful dogshit. but it works :)
            if (_mind.TryFindObjective((args.Mind, mind), "HereticSacrificeObjective", out var crewObj)
            && args.EntityManager.TryGetComponent<HereticSacrificeConditionComponent>(crewObj, out var crewObjComp))
                crewObjComp.Sacrificed += 1;

            if (_mind.TryFindObjective((args.Mind, mind), "HereticSacrificeHeadObjective", out var crewHeadObj)
            && args.EntityManager.TryGetComponent<HereticSacrificeConditionComponent>(crewHeadObj, out var crewHeadObjComp)
            && isCommand)
                crewHeadObjComp.Sacrificed += 1;
        }

        if (knowledgeGain > 0)
            _heretic.UpdateMindKnowledge((args.Mind, args.Mind.Comp, store, mind), args.Performer, knowledgeGain);

        // reset it because it refuses to work otherwise.
        uids = new();
        args.EntityManager.EventBus.RaiseLocalEvent(args.Mind, new EventHereticUpdateTargets());
    }
}
