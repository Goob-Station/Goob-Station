using Content.Server.Item;
using Content.Shared._EinsteinEngines.TelescopicBaton;
using Content.Shared._White.Standing;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Timing;
using Content.Shared.Weapons.Melee.Events;
using Robust.Server.GameObjects;

namespace Content.Server._EinsteinEngines.TelescopicBaton;

public sealed class TelescopicBatonSystem : EntitySystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly ItemToggleSystem _toggle = default!; // Goobstation
    [Dependency] private readonly ItemSystem _item = default!; // Goobstation
    [Dependency] private readonly UseDelaySystem _delay = default!; // Goobstation

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TelescopicBatonComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<TelescopicBatonComponent, ItemToggledEvent>(OnToggled);
        SubscribeLocalEvent<TelescopicBatonComponent, KnockdownOnHitAttemptEvent>(OnKnockdownAttempt);
        SubscribeLocalEvent<TelescopicBatonComponent, MeleeHitEvent>(OnMeleeHit, after: new[] { typeof(KnockdownOnHitSystem) }); // Goobstation
    }

    private void OnMeleeHit(Entity<TelescopicBatonComponent> ent, ref MeleeHitEvent args) // Goobstation
    {
        if (!ent.Comp.AlwaysDropItems)
            ent.Comp.CanDropItems = false; // Goob edit

        if (args is { IsHit: true, HitEntities.Count: > 0 } && TryComp(ent, out UseDelayComponent? delay))
            _delay.ResetAllDelays((ent, delay));
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<TelescopicBatonComponent>();
        while (query.MoveNext(out var baton))
        {
            if (baton.AlwaysDropItems) // Goobstation
                continue;

            if (!baton.CanDropItems) // Goob edit
                continue;

            baton.TimeframeAccumulator += TimeSpan.FromSeconds(frameTime);
            if (baton.TimeframeAccumulator <= baton.AttackTimeframe)
                continue;

            baton.CanDropItems = false; // Goob edit
            baton.TimeframeAccumulator = TimeSpan.Zero;
        }
    }

    private void OnMapInit(Entity<TelescopicBatonComponent> baton, ref MapInitEvent args)
    {
        ToggleBaton(baton, false);
    }

    private void OnToggled(Entity<TelescopicBatonComponent> baton, ref ItemToggledEvent args)
    {
        _item.SetHeldPrefix(baton, args.Activated ? "on" : "off"); // Goobstation
        ToggleBaton(baton, args.Activated);
    }

    private void OnKnockdownAttempt(Entity<TelescopicBatonComponent> baton, ref KnockdownOnHitAttemptEvent args)
    {
        // Goob edit start
        if (!_toggle.IsActivated(baton.Owner))
        {
            args.Cancelled = true;
            return;
        }

        if (!baton.Comp.CanDropItems)
            args.Behavior = DropHeldItemsBehavior.NoDrop;
        // Goob edit end
    }

    public void ToggleBaton(Entity<TelescopicBatonComponent> baton, bool state)
    {
        baton.Comp.TimeframeAccumulator = TimeSpan.Zero;
        baton.Comp.CanDropItems = state; // Goob edit
        _appearance.SetData(baton, TelescopicBatonVisuals.State, state);
    }
}
