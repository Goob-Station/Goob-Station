using Content.Server.Stunnable.Components;
using Content.Shared.StatusEffect;
using JetBrains.Annotations;
using Content.Shared.Throwing;
using Content.Shared.Whitelist;
using Robust.Shared.Physics.Events;

namespace Content.Server.Stunnable.Systems;

[UsedImplicitly]
internal sealed class StunOnCollideSystem : EntitySystem
{
    [Dependency] private readonly StunSystem _stunSystem = default!;
    [Dependency] private readonly EntityWhitelistSystem _entityWhitelist = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<StunOnCollideComponent, StartCollideEvent>(HandleCollide);
        SubscribeLocalEvent<StunOnCollideComponent, ThrowDoHitEvent>(HandleThrow);
    }

    private void TryDoCollideStun(Entity<StunOnCollideComponent> ent, EntityUid target)
    {
        if (!EntityManager.TryGetComponent<StatusEffectsComponent>(target, out var status) ||
            ent.Comp.Blacklist is { } blacklist && _entityWhitelist.IsValid(blacklist, target))
            return;

        _stunSystem.TryStun(target, ent.Comp.StunAmount, true, status);
        _stunSystem.TryKnockdown(target, ent.Comp.KnockdownAmount, true, status);

        _stunSystem.TrySlowdown(
            target,
            ent.Comp.SlowdownAmount,
            true,
            ent.Comp.WalkSpeedMultiplier,
            ent.Comp.RunSpeedMultiplier,
            status);
    }

    private void HandleCollide(Entity<StunOnCollideComponent> ent, ref StartCollideEvent args)
    {
        if (args.OurFixtureId != ent.Comp.FixtureId)
            return;

        TryDoCollideStun(ent, args.OtherEntity);
    }

    private void HandleThrow(Entity<StunOnCollideComponent> ent, ref ThrowDoHitEvent args) =>
        TryDoCollideStun(ent, args.Target);
}
