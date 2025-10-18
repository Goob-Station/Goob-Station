using Content.Shared.StatusEffect;
using Content.Shared.Whitelist;
using Robust.Shared.Physics.Events;

namespace Content.Goobstation.Shared.Wraith.Collisions;

public abstract class SharedStatusEffectOnCollideGhostSystem : EntitySystem
{
    [Dependency] private readonly StatusEffectsSystem _statusEffectsSystem = default!;
    [Dependency] private readonly EntityWhitelistSystem _entityWhitelist = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StatusEffectOnCollideGhostComponent, StartCollideEvent>(OnCollide);
    }

    private void OnCollide(Entity<StatusEffectOnCollideGhostComponent> ent, ref StartCollideEvent args)
    {
        if (ent.Comp.Whitelist is {} whitelist
            && !_entityWhitelist.IsValid(whitelist, args.OtherEntity))
            return;

        _statusEffectsSystem.TryAddStatusEffect(
            args.OtherEntity,
            ent.Comp.StatusEffect,
            ent.Comp.Duration,
            ent.Comp.Refresh,
            ent.Comp.Component);

        var ev = new StatusEffectOnCollideEvent(ent.Comp.Duration);
        RaiseLocalEvent(args.OtherEntity, ref ev);
    }
}
