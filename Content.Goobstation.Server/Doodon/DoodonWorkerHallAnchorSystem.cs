using Content.Goobstation.Shared.Doodons;
using Content.Server.NPC;
using Content.Server.NPC.Systems;
using Content.Shared.Mind;
using Robust.Shared.Map;
using System.Numerics;

namespace Content.Goobstation.Server.Doodons;

public sealed class DoodonWorkerHallAnchorSystem : EntitySystem
{
    [Dependency] private readonly NPCSystem _npc = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;

    private const float Interval = 1.0f;
    private float _accum;

    public override void Update(float frameTime)
    {
        _accum += frameTime;
        if (_accum < Interval)
            return;
        _accum = 0f;

        var query = EntityQueryEnumerator<DoodonComponent>();
        while (query.MoveNext(out var uid, out var doodon))
        {
            // Only workers (exclude moodons + warriors)
            if (doodon.RequiredHousing != DoodonHousingType.Worker)
                continue;

            if (doodon.TownHall is not { } hallUid || Deleted(hallUid))
                continue;

            // If hall is feral, clear follow anchor so they don’t leash back in
            if (HasComp<DoodonTownHallFeralComponent>(hallUid))
            {
                _npc.SetBlackboard(uid, NPCBlackboard.FollowTarget, default(EntityCoordinates));
                continue;
            }

            // Set FollowTarget to hall center
            var hallFollow = new EntityCoordinates(hallUid, Vector2.Zero);
            _npc.SetBlackboard(uid, NPCBlackboard.FollowTarget, hallFollow);
        }
    }
}
