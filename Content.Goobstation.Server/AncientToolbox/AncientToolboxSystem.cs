using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Tag;
using Content.Shared.Storage;
using Content.Shared.Damage;
using Content.Shared.Stacks;
using Robust.Shared.Prototypes;
using Robust.Shared.Containers;
using Content.Goobstation.Shared.AncientToolbox;

namespace Content.Goobstation.Server.AncientToolbox;

public sealed class AncientToolboxSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [ValidatePrototypeId<TagPrototype>]
    private readonly ProtoId<TagPrototype> _telecrystal = "Telecrystal";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AncientToolboxComponent, EntInsertedIntoContainerMessage>(OnContainerChanged);
        SubscribeLocalEvent<AncientToolboxComponent, EntRemovedFromContainerMessage>(OnContainerChanged);
        SubscribeLocalEvent<AncientToolboxComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnContainerChanged(EntityUid uid, AncientToolboxComponent comp, ref EntInsertedIntoContainerMessage args)
    {
        if (_tag.HasTag(args.Entity, _telecrystal))
        {
            RecalculateBonusDamage(uid, comp);
            //Logger.Info("added");
        }
    }

    private void OnContainerChanged(EntityUid uid, AncientToolboxComponent comp, ref EntRemovedFromContainerMessage args)
    {
        if (_tag.HasTag(args.Entity, _telecrystal))
        {
            RecalculateBonusDamage(uid, comp);
            //Logger.Info("removed");
        }
    }

    private void RecalculateBonusDamage(EntityUid uid, AncientToolboxComponent comp)
    {
        var totalCrystals = 0;
        if (TryComp(uid, out StorageComponent? storage))
        {
            foreach (var item in storage.Container.ContainedEntities)
            {
                if (_tag.HasTag(item, _telecrystal))
                {
                    if (TryComp<StackComponent>(item, out var stack))
                        totalCrystals += stack.Count;
                    else
                        totalCrystals += 1;
                }
            }
        }
        comp.BonusDamage = comp.CrystalsPerDamageBonus == 0 ? 0 : (int) Math.Floor(totalCrystals / comp.CrystalsPerDamageBonus);
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
