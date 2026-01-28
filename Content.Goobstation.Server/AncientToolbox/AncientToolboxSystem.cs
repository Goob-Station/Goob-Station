using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Tag;
using Content.Shared.Storage;
using Content.Shared.Damage;
using Content.Shared.Stacks;
using Robust.Shared.Prototypes;
using Robust.Shared.Containers;

namespace Content.Goobstation.Server.AncientToolbox;

public sealed class AncientToolboxSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tag = default!;
    [ValidatePrototypeId<TagPrototype>]
    private readonly ProtoId<TagPrototype> TC = "Telecrystal";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AncientToolboxComponent, EntInsertedIntoContainerMessage>(OnContainerChanged);
        SubscribeLocalEvent<AncientToolboxComponent, EntRemovedFromContainerMessage>(OnContainerChanged);
        SubscribeLocalEvent<AncientToolboxComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnContainerChanged(EntityUid uid, AncientToolboxComponent comp, ref EntInsertedIntoContainerMessage args)
    {
        RecalculateBonus(uid, comp);
    }

    private void OnContainerChanged(EntityUid uid, AncientToolboxComponent comp, ref EntRemovedFromContainerMessage args)
    {
        RecalculateBonus(uid, comp);
    }

    private void RecalculateBonus(EntityUid uid, AncientToolboxComponent comp)
    {
        var totalCrystals = 0;
        if (TryComp(uid, out StorageComponent? storage))
        {
            foreach (var item in storage.Container.ContainedEntities)
            {
                if (_tag.HasTag(item, TC))
                {
                    if (TryComp<StackComponent>(item, out var stack))
                        totalCrystals += stack.Count;
                    else
                        totalCrystals += 1;
                }
            }
        }
        comp.BonusDamage = comp.CrystalsPerDamageBonus == 0 ? 0 : totalCrystals / comp.CrystalsPerDamageBonus;
    }

    private void OnMeleeHit(Entity<AncientToolboxComponent> ent, ref MeleeHitEvent args)
    {
        if (args.Handled)
            return;
        var comp = ent.Comp;
        if (comp.BonusDamage <= 0)
            return;
        var spec = new DamageSpecifier();
        spec.DamageDict.Add("Blunt", comp.BonusDamage);
        args.BonusDamage += spec;
    }
}
