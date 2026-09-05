using Content.Goobstation.Common.Gravity;
using Content.Shared.Gravity;

namespace Content.Goobstation.Shared.Gravity;

public sealed class FlipGravitySystem : EntitySystem
{
    [Dependency] private readonly SharedGravitySystem _gravity = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FlipGravityComponent, ComponentStartup>(OnFlipStartup);
        SubscribeLocalEvent<FlipGravityComponent, ComponentShutdown>(OnFlipShutdown);
        SubscribeLocalEvent<FlipGravityComponent, IsWeightlessEvent>(OnIsWeightless);
    }

    private void OnFlipStartup(Entity<FlipGravityComponent> ent, ref ComponentStartup args)
    {
        _gravity.RefreshWeightless(ent.Owner);
    }

    private void OnFlipShutdown(Entity<FlipGravityComponent> ent, ref ComponentShutdown args)
    {
        _gravity.RefreshWeightless(ent.Owner);
    }

    private void OnIsWeightless(Entity<FlipGravityComponent> ent, ref IsWeightlessEvent args)
    {
        if (args.Handled)
            return;

        var grav = _gravity.EntityGridOrMapHaveGravity(ent.Owner);
        args.IsWeightless = grav;
        args.Handled = true;
    }
}
