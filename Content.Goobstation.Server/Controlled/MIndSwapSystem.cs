using Content.Server.GameTicking;
using Content.Server.Ghost;
using Content.Server.Mind;
using Content.Shared.Ghost;
using Content.Shared.Mind.Components;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Controlled;

public sealed class MindSwapSystem : EntitySystem
{
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly GhostSystem _ghosts = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private List<EntityUid> _reverted = new();

    public void SwapMinds(EntityUid ent1, EntityUid ent2, float? duration)
    {
        if (HasComp<MindSwappedComponent>(ent1) || HasComp<MindSwappedComponent>(ent2))
        {
            Log.Warning("Tried to swap already swapped minds");
            return;
        }

        // Get minds
        _mind.TryGetMind(ent1, out var mindId1, out var mind1);
        _mind.TryGetMind(ent2, out var mindId2, out var mind2);

        // Swap them
        _mind.TransferTo(mindId1, ent2, createGhost: false);
        _mind.TransferTo(mindId2, ent1, createGhost: false);

        if (duration != null)
        {
            EnsureComp<MindSwappedComponent>(ent1).RevertTime = _timing.CurTime + TimeSpan.FromSeconds(duration.Value);
            EnsureComp<MindSwappedComponent>(ent2).RevertTime = _timing.CurTime + TimeSpan.FromSeconds(duration.Value);
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MindSwappedComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.RevertTime == null || comp.RevertTime > _timing.CurTime)
                continue;

            if (_reverted.Contains(uid))
                continue;

            if (!comp.OriginalBody.HasValue)
            {
                var ghost = Spawn(GameTicker.ObserverPrototypeName, Transform(uid).Coordinates);
                EnsureComp<MindContainerComponent>(ghost);
                var ghostComponent = Comp<GhostComponent>(ghost);
                _ghosts.SetCanReturnToBody((ghost, ghostComponent), false);
                continue;
            }

            // just swapping them the same way
            _mind.TryGetMind(uid, out var mindId1, out var mind1);
            _mind.TryGetMind(comp.OriginalBody.Value, out var mindId2, out var mind2);
            _mind.TransferTo(mindId1, comp.OriginalBody.Value, createGhost: false);
            _mind.TransferTo(mindId2, uid, createGhost: false);

            RemCompDeferred(uid, comp);
            RemCompDeferred<MindSwappedComponent>(comp.OriginalBody.Value);
            _reverted.Add(uid);
            _reverted.Add(comp.OriginalBody.Value);
        }
    }
}
