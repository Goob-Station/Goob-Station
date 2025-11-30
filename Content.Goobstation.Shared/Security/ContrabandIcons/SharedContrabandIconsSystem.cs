using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Shared.Contraband;
using Content.Goobstation.Shared.Security.ContrabandIcons.Components;
using Content.Goobstation.Shared.Security.ContrabandIcons.Prototypes;
using Content.Shared.Hands;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Strip.Components;
using Robust.Shared.Configuration;

namespace Content.Goobstation.Shared.Security.ContrabandIcons;

/// <summary>
/// This handles...
/// </summary>
public abstract class SharedContrabandIconsSystem : EntitySystem
{
    [Dependency] private readonly SharedContrabandDetectorSystem _detectorSystem = default!;
    [Dependency] private readonly IConfigurationManager _configuration = default!;
    private bool _isEnabled = true;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<VisibleContrabandComponent, DidEquipEvent>(OnEquip);
        SubscribeLocalEvent<VisibleContrabandComponent, DidUnequipEvent>(OnUnequip);

        SubscribeLocalEvent<VisibleContrabandComponent, DidEquipHandEvent>(OnEquipHands);
        SubscribeLocalEvent<VisibleContrabandComponent, DidUnequipHandEvent>(OnUnequipHands);
        
        Subs.CVar(_configuration, GoobCVars.ContrabandIconsEnabled, value => _isEnabled = value);
    }

    protected void ContrabandDetect(EntityUid inventory, VisibleContrabandComponent component, SlotFlags slotFlags = SlotFlags.WITHOUT_POCKET)
    {
        if (!_isEnabled)
            return;
        if (HasComp<ThievingComponent>(inventory))
            return;
        var list = _detectorSystem.FindContraband(inventory, false, slotFlags);
        var isDetected = list.Count > 0;
        component.StatusIcon = StatusToIcon(isDetected ? ContrabandStatus.Contraband : ContrabandStatus.None);
        Dirty(inventory, component);
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
        ContrabandDetect(uid, component, args.SlotFlags);
    }

    private void OnUnequip(EntityUid uid, VisibleContrabandComponent component, DidUnequipEvent args)
    {
        ContrabandDetect(uid, component, args.SlotFlags);
    }

    private void OnUnequipHands(EntityUid uid, VisibleContrabandComponent component, DidUnequipHandEvent args)
    {
        ContrabandDetect(uid, component, SlotFlags.NONE);
    }
    private void OnEquipHands(EntityUid uid, VisibleContrabandComponent component, DidEquipHandEvent args)
    {
        ContrabandDetect(uid, component, SlotFlags.NONE);
    }
}
