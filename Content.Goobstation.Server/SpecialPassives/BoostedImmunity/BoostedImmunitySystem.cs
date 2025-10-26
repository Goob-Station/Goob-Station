using Content.Goobstation.Shared.SpecialPassives.BoostedImmunity.Components;
using Content.Server._White.Xenomorphs.Infection;
using Content.Server.Body.Systems;
using Content.Server.GameTicking;
using Content.Shared.Body.Part;
using Content.Shared.Traits;
using Robust.Server.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.SpecialPassives.BoostedImmunity;

public sealed class BoostedImmunitySystem : SharedBoostedImmunitySystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly GameTicker _ticker = default!;

    private EntityQuery<XenomorphInfectionComponent> _xenoInfectQuery;

    public override void Initialize()
    {
        base.Initialize();

        _xenoInfectQuery = GetEntityQuery<XenomorphInfectionComponent>();

    }

    public readonly ProtoId<TraitCategoryPrototype> DisabilityProto = "Disabilities";
    protected override void RemoveDisabilities(Entity<BoostedImmunityComponent> ent)
    {
        _playerManager.TryGetSessionByEntity(ent, out var session);

        if (session == null)
            return;

        var profile = _ticker.GetPlayerProfile(session);

        foreach (var trait in profile.TraitPreferences)
        {
            if (!_protoManager.TryIndex<TraitPrototype>(trait, out var traitProto))
                continue;

            if (traitProto.Category != DisabilityProto)
                continue;

            var comps = traitProto.Components;
            EntityManager.RemoveComponents(ent, comps);
        }
    }

    protected override void RemoveAlienEmbryo(Entity<BoostedImmunityComponent> ent)
    {
        var chest = _body.GetBodyChildrenOfType(ent, BodyPartType.Chest, symmetry: BodyPartSymmetry.None).FirstOrNull();

        if (chest == null)
            return;

        var organs = _body.GetPartOrgans(chest.Value.Id);

        foreach (var organ in organs)
        {
            if (!_xenoInfectQuery.HasComp(organ.Id))
                continue;

            _body.TryRemoveOrgan(organ.Id);
            QueueDel(organ.Id);
        }
    }
}
