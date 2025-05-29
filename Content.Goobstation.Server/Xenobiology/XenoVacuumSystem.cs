using Content.Goobstation.Shared.Xenobiology;
using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Server.Popups;
using Content.Shared.Inventory;
using Content.Shared.Nyanotrasen.Item.PseudoItem;
using Content.Shared.Tag;
using Robust.Shared.Containers;

namespace Content.Goobstation.Server.Xenobiology;

/// <summary>
/// This handles...
/// </summary>
public sealed class XenoVacuumSystem : EntitySystem
{
    [Dependency] private readonly SharedPseudoItemSystem _pseudoSystem = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly TagSystem _tagSystem = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<XenoVacEvent>(OnXenoVacuum);
        SubscribeLocalEvent<XenoVacClearEvent>(OnXenoVacuumClear);
    }

    public void OnXenoVacuum(ref XenoVacEvent args)
    {
        //needs sfx also cursed code
        var user = args.Performer;
        var target = args.Target;
        var back = _inventorySystem.GetHandOrInventoryEntities(user, SlotFlags.BACK);

        if (!HasComp<SlimeComponent>(target))
            return;

        foreach (var ent in back)
        {
            if (!_tagSystem.HasTag(ent, "XenoNozzleBackTank")
                || !TryComp<PseudoItemComponent>(target, out var pseudo))
                return;

            if (!_pseudoSystem.TryInsert(ent, target, pseudo))
            {
                 var failPopup = Loc.GetString("target is not valid");
                 _popup.PopupEntity(failPopup, user);

                 return;
            }
        }
    }

    public void OnXenoVacuumClear(ref XenoVacClearEvent args)
    {
        var user = args.Performer;
        var back = _inventorySystem.GetHandOrInventoryEntities(user, SlotFlags.BACK);

        foreach (var ent in back)
        {
            if (!_tagSystem.HasTag(ent, "XenoNozzleBackTank"))
                return;

            var container = _containerSystem.GetContainer(ent, "storagebase");

            if (container.ContainedEntities.Count > 0)
                _containerSystem.EmptyContainer(container);
        }
    }

}
