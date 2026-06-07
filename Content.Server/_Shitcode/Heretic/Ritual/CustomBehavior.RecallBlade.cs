using Content.Shared.Heretic;
using Content.Shared.Heretic.Prototypes;
using Robust.Server.GameObjects;

namespace Content.Server.Heretic.Ritual;

public sealed partial class RitualRecallBladeBehavior : RitualCustomBehavior
{
    public override bool Execute(RitualData args, out string? outstr)
    {
        outstr = null;

        var entMan = args.EntityManager;
        var heretic = args.Mind.Comp;

        var transform = entMan.System<TransformSystem>();
        if (GetLostBlade(args.Platform, args.Performer, heretic, args.EntityManager, transform) != null)
            return true;

        outstr = Loc.GetString("heretic-ritual-fail-no-lost-blades");
        return false;
    }

    public override void Finalize(RitualData args)
    {
        var entMan = args.EntityManager;
        var heretic = args.Mind.Comp;

        var transform = entMan.System<TransformSystem>();
        if (GetLostBlade(args.Platform, args.Performer, heretic, args.EntityManager, transform) is not { } blade)
            return;

        transform.AttachToGridOrMap(blade);
        transform.SetMapCoordinates(blade, transform.GetMapCoordinates(args.Platform));
    }

    private EntityUid? GetLostBlade(EntityUid origin,
        EntityUid heretic,
        HereticComponent comp,
        IEntityManager entMan,
        TransformSystem transform)
    {
        if (comp.CurrentPath is not { } path || !comp.LimitedTransmutations.TryGetValue($"Blade{path}", out var blades))
            return null;

        var originCoords = transform.GetMapCoordinates(origin);
        var hereticCoords = transform.GetMapCoordinates(heretic);

        if (originCoords.MapId != hereticCoords.MapId)
            return null;

        var dist = (originCoords.Position - hereticCoords.Position).Length();

        var range = MathF.Max(1.5f, dist + 0.5f);

        foreach (var blade in blades)
        {
            if (!entMan.EntityExists(blade))
                continue;

            if (originCoords.InRange(transform.GetMapCoordinates(blade), range))
                continue;

            return blade;
        }

        return null;
    }
}
