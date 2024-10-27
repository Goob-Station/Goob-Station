using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Stunnable.Events;
using Content.Shared.TelescopicBaton;
using Robust.Server.GameObjects;

namespace Content.Server.TelescopicBaton;

public sealed class TelescopicBatonSystem : EntitySystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TelescopicBatonComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<TelescopicBatonComponent, ItemToggledEvent>(OnToggled);
        SubscribeLocalEvent<TelescopicBatonComponent, KnockdownOnHitAttemptEvent>(OnKnockdownAttempt);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<TelescopicBatonComponent>();
        while (query.MoveNext(out var baton))
        {
            if (!baton.CanKnockDown)
                continue;

            baton.TimeframeAccumulator += TimeSpan.FromSeconds(frameTime);
            if (baton.TimeframeAccumulator <= baton.AttackTimeframe)
                continue;

            baton.CanKnockDown = false; // Only disable knockdown
            baton.TimeframeAccumulator = TimeSpan.Zero;
        }
    }

    private void OnMapInit(Entity<TelescopicBatonComponent> baton, ref MapInitEvent args)
    {
        ToggleBaton(baton, false);
    }

    private void OnToggled(Entity<TelescopicBatonComponent> baton, ref ItemToggledEvent args)
    {
        ToggleBaton(baton, args.Activated);
    }

    private void OnKnockdownAttempt(Entity<TelescopicBatonComponent> baton, ref KnockdownOnHitAttemptEvent args)
    {
        if (!baton.Comp.CanKnockDown)
        {
            args.Cancelled = true;
            return;
        }

        baton.Comp.CanKnockDown = false;
    }

    public void ToggleBaton(Entity<TelescopicBatonComponent> baton, bool state)
    {
        baton.Comp.TimeframeAccumulator = TimeSpan.Zero;
        baton.Comp.CanKnockDown = state;
        _appearance.SetData(baton, TelescopicBatonVisuals.State, state);
    }
}
