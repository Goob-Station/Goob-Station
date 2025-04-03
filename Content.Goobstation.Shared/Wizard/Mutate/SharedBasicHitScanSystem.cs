using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wizard.Mutate;

public sealed class SharedBasicHitScanSystem : EntitySystem
{
    [Dependency] private readonly PrototypeManager _prototypeManager = default!;
     public override void Initialize()
    {
        SubscribeLocalEvent<BasicHitscanAmmoProviderComponent, TakeAmmoEvent>(OnBasicHitscanTakeAmmo);
        SubscribeLocalEvent<BasicHitscanAmmoProviderComponent, GetAmmoCountEvent>(OnBasicHitscanAmmoCount);
    }

    private void OnBasicHitscanAmmoCount(Entity<BasicHitscanAmmoProviderComponent> ent, ref GetAmmoCountEvent args)
    {
        args.Capacity = int.MaxValue;
        args.Count = int.MaxValue;
    }

    private void OnBasicHitscanTakeAmmo(Entity<BasicHitscanAmmoProviderComponent> ent, ref TakeAmmoEvent args)
    {
        for (var i = 0; i < args.Shots; i++)
        {
            args.Ammo.Add((null, _prototypeManager.Index(ent.Comp.Proto)));
        }
    }
}
