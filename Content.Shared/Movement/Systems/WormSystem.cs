using Content.Shared.Alert;
using Content.Shared.Movement.Components;
using Content.Shared.Popups;
using Content.Shared.Rejuvenate;
using Content.Shared.Stunnable;
using Content.Shared.Physics; // Pirate
using Robust.Shared.Physics; // Pirate
using Robust.Shared.Physics.Components; // Pirate
using Robust.Shared.Physics.Systems; // Pirate

namespace Content.Shared.Movement.Systems;

/// <summary>
/// This handles the worm component
/// </summary>
public sealed class WormSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!; // Pirate - togglable under-table crawling

    public override void Initialize()
    {
        SubscribeLocalEvent<WormComponent, StandUpAttemptEvent>(OnStandAttempt);
        SubscribeLocalEvent<WormComponent, KnockedDownRefreshEvent>(OnKnockedDownRefresh);
        SubscribeLocalEvent<WormComponent, RejuvenateEvent>(OnRejuvenate);
        SubscribeLocalEvent<WormComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<WormComponent, ComponentShutdown>(OnShutdown); // Pirate - togglable under-table crawling
    }

    private void OnMapInit(Entity<WormComponent> ent, ref MapInitEvent args)
    {
        EnsureComp<KnockedDownComponent>(ent, out var knocked);
        _alerts.ShowAlert(ent.Owner, SharedStunSystem.KnockdownAlert);
        _stun.SetAutoStand((ent, knocked));
        SetMask(ent, true); // Pirate - togglable under-table crawling
    }

    // Pirate start - togglable under-table crawling
    private void OnShutdown(Entity<WormComponent> ent, ref ComponentShutdown args)
    {
        SetMask(ent, false);
    }

    private void SetMask(EntityUid uid, bool canPass)
    {
        if (!TryComp<FixturesComponent>(uid, out var fixtures) || !TryComp<PhysicsComponent>(uid, out var physics))
            return;

        var maskBits = (int) (CollisionGroup.MidImpassable | CollisionGroup.HighImpassable);

        foreach (var (id, fixture) in fixtures.Fixtures)
        {
            if (!fixture.Hard)
                continue;

            int newMask = fixture.CollisionMask;
            if (canPass)
                newMask &= ~maskBits;
            else
                newMask |= maskBits;

            if (newMask != fixture.CollisionMask)
            {
                _physics.SetCollisionMask(uid, id, fixture, newMask, fixtures, physics);
            }
        }
    }
    // Pirate end - togglable under-table crawling

    private void OnRejuvenate(Entity<WormComponent> ent, ref RejuvenateEvent args)
    {
        RemComp<WormComponent>(ent);
    }

    private void OnStandAttempt(Entity<WormComponent> ent, ref StandUpAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        args.Cancelled = true;
        args.Message = (Loc.GetString("worm-component-stand-attempt"), PopupType.SmallCaution);
        args.Autostand = false;
    }

    private void OnKnockedDownRefresh(Entity<WormComponent> ent, ref KnockedDownRefreshEvent args)
    {
        args.FrictionModifier *= ent.Comp.FrictionModifier;
        args.SpeedModifier *= ent.Comp.SpeedModifier;
    }
}
