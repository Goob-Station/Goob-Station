using Content.Goobstation.Shared.Bloodsuckers.Components;
using Content.Goobstation.Shared.Bloodsuckers.Components.Actions;
using Content.Goobstation.Shared.Bloodsuckers.Events;
using Content.Shared.Physics;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using System.Numerics;

namespace Content.Goobstation.Shared.Bloodsuckers.Systems;

/// <summary>
/// Turn to mist and teleport two tiles away at most. Can't go through walls.
/// </summary>
public sealed class BloodsuckerTrespassSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly BloodsuckerHumanitySystem _humanity = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BloodsuckerComponent, BloodsuckerTrespassEvent>(OnTrespass);
    }

    private void OnTrespass(Entity<BloodsuckerComponent> ent, ref BloodsuckerTrespassEvent args)
    {
        if (!TryComp(ent, out BloodsuckerTrespassComponent? comp))
            return;

        if (!TryUseCosts(ent, comp))
            return;

        if (!TryComp(ent.Owner, out TransformComponent? xform))
            return;

        var origin = xform.WorldPosition;
        var destination = args.Target.Position;

        var delta = destination - origin;
        if (delta.Length() > comp.TeleportRange)
            destination = origin + Vector2.Normalize(delta) * comp.TeleportRange;

        if (IsBlockedByWall(xform.MapID, origin, destination))
            return;

        //var mistEv = new BloodsuckerTrespassMistEvent(origin, destination);
        //RaiseLocalEvent(ent.Owner, ref mistEv);

        _transform.SetWorldPosition(ent.Owner, destination);

        _audio.PlayPvs(comp.TeleportSound, ent.Owner);
    }

    private bool IsBlockedByWall(MapId mapId, Vector2 from, Vector2 to)
    {
        var steps = (int) ((to - from).Length() / 0.4f) + 1;
        for (int i = 1; i <= steps; i++)
        {
            var point = Vector2.Lerp(from, to, (float) i / steps);
            var half = new Vector2(0.1f, 0.1f);
            var box = new Box2(point - half, point + half);

            foreach (var hit in _lookup.GetEntitiesIntersecting(mapId, box))
            {
                if (!TryComp(hit, out FixturesComponent? fixtures))
                    continue;

                foreach (var (_, fixture) in fixtures.Fixtures)
                {
                    if ((fixture.CollisionLayer & (int) CollisionGroup.Impassable) != 0)
                        return true;
                }
            }
        }
        return false;
    }

    private bool TryUseCosts(Entity<BloodsuckerComponent> ent, BloodsuckerTrespassComponent comp)
    {
        if (comp.DisabledInFrenzy && HasComp<BloodsuckerFrenzyComponent>(ent))
            return false;

        if (comp.HumanityCost != 0f && TryComp(ent, out BloodsuckerHumanityComponent? humanity))
            _humanity.ChangeHumanity(new Entity<BloodsuckerHumanityComponent>(ent.Owner, humanity), -comp.HumanityCost);

        return true;
    }
}
