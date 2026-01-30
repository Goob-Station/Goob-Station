using Content.Goobstation.Common.CCVar;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Mind;
using Content.Server.Popups;
using Content.Server.Warps;
using Content.Shared.Mobs.Components;
using Content.Shared.Rejuvenate;
using Robust.Server.GameObjects;
using Robust.Shared.Configuration;
using Content.Shared.Popups;

namespace Content.Goobstation.Server.CentComm;
public sealed partial class WarpParentOnTriggerSystem : EntitySystem
{
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WarpParentOnTriggerComponent, TriggerEvent>(OnTrigger);
    }
    public void OnTrigger(Entity<WarpParentOnTriggerComponent> ent, ref TriggerEvent args)
    {
        if (!WarpParent(ent))
        {
            _popup.PopupEntity(Loc.GetString("lifeline-trigger-fail"), ent.Owner, PopupType.Medium);
            EntityManager.QueueDeleteEntity(Transform(ent.Owner).ParentUid);
        }

        args.Handled = true;
    }

    private bool WarpParent(Entity<WarpParentOnTriggerComponent> ent)
    {
        var location = FindWarpPoint(ent.Comp.WarpLocation);

        if (location == null)
            return false;

        var transform = Transform(ent.Owner);
        var parentUid = transform.ParentUid;

        if (parentUid == EntityUid.Invalid || !HasComp<MobStateComponent>(parentUid))
            return false;

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
        return true;
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
