using Content.Goobstation.Shared.Contraband;
using Content.Goobstation.Shared.Security.ContrabandIcons.Components;
using Content.Shared.Contraband;
using Content.Shared.GameTicking;
using Content.Shared.Hands;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Robust.Shared.Containers;
using Content.Goobstation.Shared.Inventory;
using Content.Goobstation.Shared.Security.ContrabandIcons.Prototypes;
using Content.Shared.Access.Components;

namespace Content.Shared._Goobstation.Security.ContrabandIcons;

/// <summary>
/// This handles...
/// </summary>
public abstract class SharedContrabandIconsSystem : EntitySystem
{
    [Dependency] private readonly SharedContrabandDetectorSystem _detectorSystem = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<VisibleContrabandComponent, DidEquipEvent>(OnEquip);
        SubscribeLocalEvent<VisibleContrabandComponent, DidUnequipEvent>(OnUnequip);

        SubscribeLocalEvent<VisibleContrabandComponent, DidEquipHandEvent>(OnEquipHands);
        SubscribeLocalEvent<VisibleContrabandComponent, DidUnequipHandEvent>(OnUnequippHands);
    }
    public void ContrabandDetect(EntityUid intentory, VisibleContrabandComponent component, SlotFlags slotFlags = SlotFlags.WITHOUT_POCKET)
    {
        var list = _detectorSystem.FindContraband(intentory, false, slotFlags);
        bool isDetected = list.Count > 0;
        component.StatusIcon = StatusToIcon(isDetected ? ContrabandStatus.Contraband : ContrabandStatus.None);
        Dirty(intentory, component);
    }
    private string StatusToIcon(ContrabandStatus status)
    {
        return status switch
        {
            ContrabandStatus.None => "ContrabandIconNone",
            ContrabandStatus.Contraband => "ContrabandIconContraband",
            _ => "ContrabandIconNone"
        };
    }

    private void OnEquip(EntityUid uid, VisibleContrabandComponent component, DidEquipEvent args)
    {
        ContrabandDetect(args.Equipee, component, args.SlotFlags);
    }

    private void OnUnequip(EntityUid uid, VisibleContrabandComponent component, DidUnequipEvent args)
    {
        ContrabandDetect(args.Equipee, component, args.SlotFlags);
    }

    private void OnUnequippHands(EntityUid uid, VisibleContrabandComponent component, DidUnequipHandEvent args)
    {
        if (HasComp<IdCardComponent>(args.Unequipped) && uid == args.User)
            return;
        ContrabandDetect(args.User, component);
    }

    private void OnEquipHands(EntityUid uid, VisibleContrabandComponent component, DidEquipHandEvent args)
    {
        ContrabandDetect(args.User, component);
    }
}
