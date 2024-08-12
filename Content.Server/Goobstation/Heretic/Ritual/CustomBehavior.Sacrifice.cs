using Content.Shared.Heretic.Prototypes;
using Content.Shared.Changeling;
using Content.Shared.Mobs.Components;
using Robust.Shared.Prototypes;
using Content.Shared.Humanoid;
using Content.Server.Revolutionary.Components;
using Content.Server.Objectives.Components;
using Content.Shared.Mind;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Heretic;
using Content.Server.Heretic.EntitySystems;
using Robust.Server.Prototypes;

namespace Content.Server.Heretic.Ritual;

/// <summary>
///     Checks for a nearest dead body,
///     gibs it and gives the heretic knowledge points.
/// </summary>
// these classes should be lead out and shot
public sealed partial class RitualSacrificeBehavior : RitualCustomBehavior
{
    // this is awful but it works so i'm not complaining
    private SharedMindSystem _mind = default!;
    private HereticSystem _heretic = default!;
    private DamageableSystem _damage = default!;
    private EntityLookupSystem _lookup = default!;
    [Dependency] private IPrototypeManager _proto = default!;

    private readonly List<EntityUid> uids = new();

    public override bool Execute(RitualData args, out string? outstr)
    {
        _mind = args.EntityManager.System<SharedMindSystem>();
        _heretic = args.EntityManager.System<HereticSystem>();
        _damage = args.EntityManager.System<DamageableSystem>();
        _lookup = args.EntityManager.System<EntityLookupSystem>();
        _proto = IoCManager.Resolve<IPrototypeManager>();

        if (!args.EntityManager.TryGetComponent<HereticComponent>(args.Performer, out var hereticComp))
        {
            outstr = string.Empty;
            return false;
        }

        var lookup = _lookup.GetEntitiesInRange(args.Platform, 0.5f);

        foreach (var look in lookup)
        {
            // get the first dead one
            if (!args.EntityManager.TryGetComponent<MobStateComponent>(look, out var mobstate)
            || !args.EntityManager.HasComponent<HumanoidAppearanceComponent>(look))
                continue;

            // eldritch gods don't want these nature freaks
            if (args.EntityManager.HasComponent<ChangelingComponent>(look))
                continue;

            if (mobstate.CurrentState == Shared.Mobs.MobState.Dead)
                uids.Add(look);
        }

        if (uids.Count == 0)
        {
            outstr = Loc.GetString("heretic-ritual-fail-sacrifice");
            return false;
        }

        outstr = null;
        return true;
    }

    public override void Finalize(RitualData args)
    {
        foreach (var acc in uids)
        {
            var knowledgeGain = args.EntityManager.HasComponent<CommandStaffComponent>(acc) ? 2f : 1f;

            if (_mind.TryGetMind(args.Performer, out var mindId, out var mind)
            && _mind.TryGetObjectiveComp<HereticSacrificeConditionComponent>(mindId, out var objective, mind))
            {
                if (args.EntityManager.HasComponent<CommandStaffComponent>(acc) && objective.IsCommand)
                    objective.Sacrificed += 1;
                objective.Sacrificed += 1; // give one nontheless
            }

            if (args.EntityManager.TryGetComponent<HereticComponent>(args.Performer, out var hereticComp))
                _heretic.UpdateKnowledge(args.Performer, hereticComp, knowledgeGain);

            // YES!!! GIB!!!
            if (args.EntityManager.TryGetComponent<DamageableComponent>(acc, out var dmg))
            {
                var prot = (ProtoId<DamageGroupPrototype>) "Brute";
                var dmgtype = _proto.Index(prot);
                // set to absurd number so that it'll 1000% get gibbed
                _damage.TryChangeDamage(acc, new DamageSpecifier(dmgtype, 2000), true);
            }
        }
    }
}
