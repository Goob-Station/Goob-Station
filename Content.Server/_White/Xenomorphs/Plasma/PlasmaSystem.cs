using Content.Shared._White.Xenomorphs.Plasma;
using Content.Shared._White.Xenomorphs.Plasma.Components;
using Content.Shared._White.Xenomorphs.Stealth;
using Content.Shared._White.Xenomorphs.Xenomorph;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Placeable;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;

namespace Content.Server._White.Xenomorphs.Plasma;

public sealed class PlasmaSystem : SharedPlasmaSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly PhysicsSystem _physics = default!;

    private EntityQuery<XenomorphComponent> _xenoQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlasmaGainModifierComponent, ItemPlacedEvent>(OnItemPlaced);
        SubscribeLocalEvent<PlasmaGainModifierComponent, ItemRemovedEvent>(OnItemRemoved);

        _xenoQuery = GetEntityQuery<XenomorphComponent>();
    }

    private void OnItemPlaced(Entity<PlasmaGainModifierComponent> ent, ref ItemPlacedEvent args)
    {
        if (!_xenoQuery.TryComp(args.OtherEntity, out var xenomorph) || xenomorph.OnWeed)
            return;

        xenomorph.OnWeed = true;
    }

    private void OnItemRemoved(Entity<PlasmaGainModifierComponent> ent, ref ItemRemovedEvent args)
    {
        if (!_xenoQuery.TryComp(args.OtherEntity, out var xenomorph) || !xenomorph.OnWeed)
            return;

        foreach (var contact in _physics.GetContactingEntities(args.OtherEntity))
        {
            if (contact == ent.Owner || !HasComp<PlasmaGainModifierComponent>(contact))
                continue;

            return;
        }

        xenomorph.OnWeed = false;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var time = _timing.CurTime;

        var query = EntityQueryEnumerator<PlasmaVesselComponent, MobStateComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var plasmaVessel, out var mobstate, out var _))
        {
            if (mobstate.CurrentState == MobState.Dead)
                continue;

            if (time < plasmaVessel.NextPointsAt)
                continue;

            plasmaVessel.NextPointsAt = time + TimeSpan.FromSeconds(1);

            var plasma = plasmaVessel.PlasmaPerSecondOffWeed;
            if (_xenoQuery.TryComp(uid, out var xenomorph) && xenomorph.OnWeed)
                plasma = plasmaVessel.PlasmaPerSecondOnWeed;

            if (TryComp<StealthOnWalkComponent>(uid, out var stealthOnWalk) && stealthOnWalk.Stealth)
                plasma -= stealthOnWalk.PlasmaCost;

            ChangePlasmaAmount(uid, plasma, plasmaVessel);
        }
    }
}
