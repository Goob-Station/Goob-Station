using Content.Goobstation.Shared.Disease.Systems;
using Content.Goobstation.Shared.Disease.Components;
using Content.Goobstation.Shared.Medical;
using Content.Goobstation.Shared.SpecialPassives.BoostedImmunity.Components;
using Content.Server._White.Xenomorphs.Infection;
using Content.Server.Body.Systems;
using Content.Shared.Body.Part;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.SpecialPassives.BoostedImmunity;

public sealed class BoostedImmunitySystem : SharedBoostedImmunitySystem
{
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly SharedDiseaseSystem _disease = default!;

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

    protected override void RemoveDiseases(Entity<BoostedImmunityComponent> ent)
    {
        _disease.TryCureAll(ent.Owner);
    }
}
