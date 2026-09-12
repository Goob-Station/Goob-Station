using System.Numerics;
using Content.Shared.Weapons.Hitscan.Events;
using Content.Shared.Weapons.Hitscan.Systems;
using Robust.Shared.Map;

namespace Content.Goobstation.Shared.Weapons.Ranged.HitscanMultiBeam;

public sealed class HitscanMultiBeamSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HitscanMultiBeamComponent, HitscanTraceEvent>(OnHitscanTrace, before: [typeof(HitscanBasicRaycastSystem)]);
    }

    private void OnHitscanTrace(Entity<HitscanMultiBeamComponent> ent, ref HitscanTraceEvent args)
    {
        if (ent.Comp.Split || ent.Comp.Beams < 2)
            return;

        ent.Comp.Split = true;

        var direction = args.ShotDirection.Normalized();
        var perpendicular = new Vector2(-direction.Y, direction.X);
        var origin = _transform.ToMapCoordinates(args.FromCoordinates);
        var centre = (ent.Comp.Beams - 1) / 2f;

        for (var i = 1; i < ent.Comp.Beams; i++)
        {
            var extra = new HitscanTraceEvent
            {
                FromCoordinates = Offset(args.FromCoordinates, origin, perpendicular * (i - centre) * ent.Comp.Spacing),
                ShotDirection = args.ShotDirection,
                Gun = args.Gun,
                Shooter = args.Shooter,
                Target = args.Target,
            };

            RaiseLocalEvent(ent, ref extra);
        }

        args.FromCoordinates = Offset(args.FromCoordinates, origin, perpendicular * -centre * ent.Comp.Spacing);
    }

    private EntityCoordinates Offset(EntityCoordinates coordinates, MapCoordinates origin, Vector2 worldOffset)
    {
        var shifted = new MapCoordinates(origin.Position + worldOffset, origin.MapId);
        return _transform.ToCoordinates(coordinates.EntityId, shifted);
    }
}
