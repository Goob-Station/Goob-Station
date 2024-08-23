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

namespace Content.Server.Heretic.Ritual;

/// <summary>
///     Checks for a nearest dead body,
///     gibs it and gives the heretic knowledge points.
/// </summary>
// these classes should be lead out and shot
public sealed partial class RitualSacrificeBehavior : RitualCustomBehavior
{
    /// <summary>
    ///     Minimal amount of corpses.
    /// </summary>
    [DataField] public float Min = 1;

    /// <summary>
    ///     Maximum amount of corpses.
    /// </summary>
    [DataField] public float Max = 1;

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
        if (lookup.Count == 0 || lookup == null)
        {
            outstr = Loc.GetString("heretic-ritual-fail-sacrifice");
            return false;
        }

        // get all the dead ones
        foreach (var look in lookup)
        {
            if (!args.EntityManager.TryGetComponent<MobStateComponent>(look, out var mobstate)
            || !args.EntityManager.HasComponent<HumanoidAppearanceComponent>(look)
            || args.EntityManager.HasComponent<ChangelingComponent>(look))
                continue;

            if (mobstate.CurrentState == Shared.Mobs.MobState.Dead)
                uids.Add(look);
        }

        if (uids.Count < Min)
        {
            outstr = Loc.GetString("heretic-ritual-fail-sacrifice");
            return false;
        }

        outstr = null;
        return true;
    }

    public override void Finalize(RitualData args)
    {
        for (int i = 0; i < Max; i++)
        {
            var knowledgeGain = args.EntityManager.HasComponent<CommandStaffComponent>(uids[i]) ? 2f : 1f;

            if (_mind.TryGetMind(args.Performer, out var mindId, out var mind)
            && _mind.TryGetObjectiveComp<HereticSacrificeConditionComponent>(mindId, out var objective, mind))
            {
                if (args.EntityManager.HasComponent<CommandStaffComponent>(uids[i]) && objective.IsCommand)
                    objective.Sacrificed += 1;
                objective.Sacrificed += 1; // give one nontheless
            }

            if (args.EntityManager.TryGetComponent<HereticComponent>(args.Performer, out var hereticComp))
                _heretic.UpdateKnowledge(args.Performer, hereticComp, knowledgeGain);

            // YES!!! GIB!!!
            if (args.EntityManager.TryGetComponent<DamageableComponent>(uids[i], out var dmg))
            {
                var prot = (ProtoId<DamageGroupPrototype>) "Brute";
                var dmgtype = _proto.Index(prot);
                _damage.TryChangeDamage(uids[i], new DamageSpecifier(dmgtype, 1984f), true);
            }
        }
    }
}
