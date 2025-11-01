using Content.Goobstation.Shared.Medical;
using Content.Goobstation.Shared.SpecialPassives.BoostedImmunity.Components;
using Content.Server._White.Xenomorphs.Infection;
using Content.Server.Body.Systems;
using Content.Server.GameTicking;
using Content.Shared.Body.Part;
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

    public readonly ProtoId<DisabilityListPrototype> DisabilityProto = "AllDisabilities";
    protected override void RemoveDisabilities(Entity<BoostedImmunityComponent> ent)
    {
        if (!_protoManager.TryIndex(DisabilityProto, out var disabilityList))
            return;

        EntityManager.RemoveComponents(ent, disabilityList.Components);
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
