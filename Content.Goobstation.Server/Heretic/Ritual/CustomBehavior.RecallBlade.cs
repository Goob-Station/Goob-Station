using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared.Heretic;
using Content.Shared.Heretic.Prototypes;
using Robust.Server.GameObjects;

namespace Content.Goobstation.Server.Heretic.Ritual;

public sealed partial class RitualRecallBladeBehavior : RitualCustomBehavior
{
    public override bool Execute(RitualData args, [NotNullWhen(true)] out string? outstr)
    {
        outstr = null;

        var entMan = args.EntityManager;
        var transform = entMan.System<TransformSystem>();

        if (!entMan.TryGetComponent(args.Performer, out HereticComponent? heretic)
            || GetLostBlade(args.Platform, args.Performer, heretic, args.EntityManager, transform) == null)
            return false;

        outstr = Loc.GetString("heretic-ritual-fail-no-lost-blades");
        return false;
    }

    public override void Finalize(RitualData args)
    {
        var entMan = args.EntityManager;
        var transform = entMan.System<TransformSystem>();
        if (!entMan.TryGetComponent(args.Performer, out HereticComponent? heretic)
            || GetLostBlade(args.Platform, args.Performer, heretic, args.EntityManager, transform) is not { } blade)
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
        var originCoords = transform.GetMapCoordinates(origin);
        var hereticCoords = transform.GetMapCoordinates(heretic);

        // Not on the same map, cancel.
        if (originCoords.MapId != hereticCoords.MapId)
            return null;

        var dist = (originCoords.Position - hereticCoords.Position).Length();
        var range = MathF.Max(1.5f, dist + 0.5f);

        foreach (var blade in comp.OurBlades
                     .Where(entMan.EntityExists)
                     .Where(blade => !originCoords.InRange(transform.GetMapCoordinates(blade), range)))
        {
            return blade;
        }

        return null;
    }
}
