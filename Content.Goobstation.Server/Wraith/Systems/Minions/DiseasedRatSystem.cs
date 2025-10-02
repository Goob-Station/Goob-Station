using Content.Goobstation.Shared.Wraith.Components.Mobs;
using Content.Server.Mind;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Systems;

/// <summary>
/// This handles the system for the diseased rat evolving into its bigger stages.
/// </summary>
public sealed class DiseasedRatSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly TransformSystem _transformSystem = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<DiseasedRatComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.FilthConsumed >= comp.MediumFilthThreshold || comp.FilthConsumed < comp.GiantFilthThreshold)
            {
                Evolve(uid, "MobPlagueRatMedium");
            }
            else if (comp.FilthConsumed >= comp.GiantFilthThreshold)
            {
                Evolve(uid, "MobPlagueRatGiant");
                // Removes the component to avoid unecessarily calling on update any further.
                RemCompDeferred<DiseasedRatComponent>(uid);
            }
        }
    }
    private void Evolve(EntityUid uid, string newProto)
    {
        if (!_proto.TryIndex(newProto, out _))
            return;

        if (!_mind.TryGetMind(uid, out var mindUid, out var mind))
            return;

        var coords = _transformSystem.GetMoverCoordinates(uid);
        var newForm = Spawn(newProto, coords);

        // Transfer player control if it had one
        _mind.TransferTo(mindUid, newForm, mind: mind);
        _mind.UnVisit(mindUid, mind);

        // Copy over remaining components
        EntityManager.CopyComponents(uid, newForm);

        Del(uid);
    }
}
