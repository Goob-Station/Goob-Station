using System.Linq;
using Content.Shared.WhiteDream.BloodCult.Components;
using Robust.Client.Placement;
using Robust.Client.Placement.Modes;
using Robust.Shared.Map;

namespace Content.Client.WhiteDream.BloodCult.UI;

public sealed class AlignPylonConstruction : SnapgridCenter
{
    [Dependency] private readonly IEntityManager _entityManager = default!;

    private readonly EntityLookupSystem _lookup;

    private const float PylonLookupRange = 10;

    public AlignPylonConstruction(PlacementManager pMan) : base(pMan)
    {
        IoCManager.InjectDependencies(this);
        _lookup = _entityManager.System<EntityLookupSystem>();
    }

    public override bool IsValidPosition(EntityCoordinates position)
    {
        return base.IsValidPosition(position) && !CheckForOtherPylons(position, PylonLookupRange);
    }

    private bool CheckForOtherPylons(EntityCoordinates coordinates, float range)
    {
        var entities = _lookup.GetEntitiesInRange(coordinates, range);
        return entities.Any(_entityManager.HasComponent<PylonComponent>);
    }
}
