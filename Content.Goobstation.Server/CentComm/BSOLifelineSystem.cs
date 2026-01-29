using Content.Goobstation.Common.CCVar;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Mind;
using Content.Server.Warps;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Rejuvenate;
using Content.Shared.Standing;
using Robust.Server.GameObjects;
using Robust.Shared.Configuration;

namespace Content.Goobstation.Server.CentComm;
public sealed partial class BSOLifelineSystem : EntitySystem
{
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BSOLifelineComponent, TriggerEvent>(OnTrigger);
    }

    public void OnTrigger(Entity<BSOLifelineComponent> ent, ref TriggerEvent args)
    {
        WarpParent(ent);
        args.Handled = true;
    }

    private void WarpParent(Entity<BSOLifelineComponent> ent)
    {
        var location = FindWarpPoint(ent.Comp.WarpLocation);

        if (location == null)
            return;

        var transform = Transform(ent.Owner);

        var parentUid = transform.ParentUid;

        if (parentUid == EntityUid.Invalid || !HasComp<MobStateComponent>(parentUid))
            return;

        // Reset mind - can be considered if greentext is a concern
        if (_config.GetCVar(GoobCVars.LifeLineResetMind))
        {
            if (_mind.TryGetMind(parentUid, out var mindId, out var mind))
            {
                var userId = mind.UserId;
                var name = mind.CharacterName;
                _mind.WipeMind(parentUid);
                var newMindId = _mind.CreateMind(userId, name).Owner;
                _mind.TransferTo(newMindId, parentUid, true);
            }
        }

        var coords = _transform.GetMapCoordinates(location.Value);
        _transform.SetMapCoordinates(parentUid, coords);

        if (_config.GetCVar(GoobCVars.LifeLineRejuvenate))
        {
            RaiseLocalEvent(parentUid, new RejuvenateEvent());
        }

        QueueDel(ent.Owner);
    }
    private EntityUid? FindWarpPoint(string location)
    {
        var query = EntityQueryEnumerator<WarpPointComponent, TransformComponent>();

        while (query.MoveNext(out var entity, out var warp, out var transform))
        {
            if (warp.Location == location)
                return entity;
        }

        return null;
    }
}
