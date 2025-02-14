using Content.Shared.Projectiles;
using Content.Shared.Whitelist;

namespace Content.Shared._Goobstation.Wizard.Projectiles;

public sealed class SwapOnProjectileHitSystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly SharedSpellsSystem _spells = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SwapOnProjectileHitComponent, ProjectileHitEvent>(OnHit);
    }

    private void OnHit(Entity<SwapOnProjectileHitComponent> ent, ref ProjectileHitEvent args)
    {
        var (uid, comp) = ent;

        if (args.Shooter == null || args.Shooter.Value == args.Target || TerminatingOrDeleted(uid))
            return;

        if (!_whitelist.IsValid(comp.Whitelist, args.Target))
            return;

        _spells.Swap(args.Shooter.Value,
            Transform(args.Shooter.Value),
            args.Target,
            Transform(args.Target),
            comp.Sound,
            comp.Effect);

        if (comp.DeleteProjectileOnSwap)
            Del(uid);
    }
}
