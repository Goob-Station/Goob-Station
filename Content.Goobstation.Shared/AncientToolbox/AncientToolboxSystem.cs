using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Tag;
using Content.Shared.Storage;
using Content.Shared.Damage;
using Content.Shared.Stacks;

namespace Content.Goobstation.Shared.AncientToolbox;

public sealed class TelecrystalDamageBoostSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tag = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AncientToolboxComponent, MeleeHitEvent>(OnMeleeHit);
    }
    private void OnMeleeHit(Entity<AncientToolboxComponent> ent, ref MeleeHitEvent args)
    {
        if (args.Handled)
            return;

        var comp = ent.Comp;
        var totalCrystals = 0;
        if (TryComp(ent.Owner, out StorageComponent? storage))
        {
            foreach (var item in storage.Container.ContainedEntities)
            {
                if (_tag.HasTag(item, "Telecrystal"))
                {
                    if (TryComp<StackComponent>(item, out var stack))
                        totalCrystals += stack.Count;
                    else
                        totalCrystals += 1;
                }
            }
        }
        var bonus = 0;
        if (comp.CrystalsPerDamageBonus != 0)
            bonus = totalCrystals / comp.CrystalsPerDamageBonus;
        if (bonus <= 0)
            return;
        var spec = new DamageSpecifier();
        spec.DamageDict.Add("Blunt", bonus);
        args.BonusDamage += spec;
    }
}
