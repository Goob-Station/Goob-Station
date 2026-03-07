using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.Access.Components;
using Content.Shared.Heretic.Prototypes;

namespace Content.Server.Heretic.Ritual;

public sealed partial class RitualEldritchIdBehavior : RitualCustomBehavior
{
    protected EntityLookupSystem _lookup = default!;

    public override bool Execute(RitualData args, out string? outstr)
    {
        outstr = null;

        _lookup = args.EntityManager.System<EntityLookupSystem>();

        if (FindIdCard(args.EntityManager, args.Platform) != null)
            return true;

        outstr = Loc.GetString("heretic-ritual-fail-no-id-card");
        return false;
    }

    public override void Finalize(RitualData args)
    {
        if (FindIdCard(args.EntityManager, args.Platform) is not { } idCard)
            return;

        args.EntityManager.EnsureComponent<EldritchIdCardComponent>(idCard);
    }

    private Entity<IdCardComponent>? FindIdCard(IEntityManager entMan, EntityUid rune)
    {
        var coords = entMan.GetComponent<TransformComponent>(rune).Coordinates;
        var lookup = _lookup.GetEntitiesInRange<IdCardComponent>(coords, 1.5f);

        var eldritchQuery = entMan.GetEntityQuery<EldritchIdCardComponent>();

        foreach (var idCard in lookup)
        {
            if (eldritchQuery.HasComp(idCard))
                continue;

            return idCard;
        }

        return null;
    }
}
