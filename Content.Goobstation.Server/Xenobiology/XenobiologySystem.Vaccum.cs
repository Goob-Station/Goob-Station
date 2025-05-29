using Content.Goobstation.Shared.Xenobiology;
using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Shared.Inventory;
using Content.Shared.Nyanotrasen.Item.PseudoItem;

namespace Content.Goobstation.Server.Xenobiology;

/// <summary>
/// This handles...
/// </summary>
public partial class XenobiologySystem
{
    private void InitializeVaccum()
    {
        SubscribeLocalEvent<XenoVacEvent>(OnXenoVacuum);
        SubscribeLocalEvent<XenoVacClearEvent>(OnXenoVacuumClear);
    }

    private void OnXenoVacuum(ref XenoVacEvent args)
    {
        //needs sfx also cursed code
        var user = args.Performer;
        var target = args.Target;
        var back = _inventorySystem.GetHandOrInventoryEntities(user, SlotFlags.BACK);

        if (!HasComp<SlimeComponent>(target))
            return;

        foreach (var ent in back)
        {
            if (!_tagSystem.HasTag(ent,
                    "XenoNozzleBackTank") // can we get these strings out of the system into a into yml -- gus :)
                || !TryComp<PseudoItemComponent>(target, out var pseudo))
                return;

            if (_pseudoSystem.TryInsert(ent, target, pseudo))
                continue;

            var failPopup = Loc.GetString("target is not valid"); // localize dummy -- gus
            _popup.PopupEntity(failPopup, user);

            return;
        }
    }

    private void OnXenoVacuumClear(ref XenoVacClearEvent args)
    {
        var user = args.Performer;
        var back = _inventorySystem.GetHandOrInventoryEntities(user, SlotFlags.BACK);

        foreach (var ent in back)
        {
            if (!_tagSystem.HasTag(ent,
                    "XenoNozzleBackTank")) // can we get these strings out of the system into a into yml -- gus :)
                return;

            var container =
                _containerSystem.GetContainer(ent,
                    "storagebase"); // can we get these strings out of the system into a into yml -- gus :)

            if (container.ContainedEntities.Count > 0)
                _containerSystem.EmptyContainer(container);
        }
    }
}
