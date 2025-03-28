using Content.Server.Gatherable.Components;
using Content.Shared.Projectiles;
using Robust.Shared.Physics.Events;
using Robust.Shared.Random; // Goobstation

namespace Content.Server.Gatherable;

public sealed partial class GatherableSystem
{
    [Dependency] private readonly IRobustRandom _robustRandom = default!; // Goobstation
    private void InitializeProjectile()
    {
        SubscribeLocalEvent<GatheringProjectileComponent, StartCollideEvent>(OnProjectileCollide);
    }

    private void OnProjectileCollide(Entity<GatheringProjectileComponent> gathering, ref StartCollideEvent args)
    {
        if (!args.OtherFixture.Hard ||
            args.OurFixtureId != SharedProjectileSystem.ProjectileFixture ||
            gathering.Comp.Amount <= 0 ||
            !TryComp<GatherableComponent>(args.OtherEntity, out var gatherable) || // Goobstation edit
            gatherable.IsGathered || // Goobstation
            !_robustRandom.Prob(gathering.Comp.Probability)) // Goobstation
        {
            return;
        }

        Gather(args.OtherEntity, gathering, gatherable);
        gatherable.IsGathered = true; // Goobstation
        gathering.Comp.Amount--;

        if (gathering.Comp.Amount <= 0)
            QueueDel(gathering);
    }
}
