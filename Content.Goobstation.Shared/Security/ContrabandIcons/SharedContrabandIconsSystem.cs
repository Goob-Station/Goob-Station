using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Shared.Contraband;
using Content.Goobstation.Shared.Security.ContrabandIcons.Components;
using Content.Shared.Hands;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Goobstation.Shared.Security.ContrabandIcons.Prototypes;
using Robust.Shared.Configuration;

namespace Content.Shared._Goobstation.Security.ContrabandIcons;

/// <summary>
/// This handles...
/// </summary>
public abstract class SharedContrabandIconsSystem : EntitySystem
{
    [Dependency] private readonly SharedContrabandDetectorSystem _detectorSystem = default!;
    [Dependency] private readonly IConfigurationManager _configuration = default!;
    bool _isEnabled = true;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<VisibleContrabandComponent, DidEquipEvent>(OnEquip);
        SubscribeLocalEvent<VisibleContrabandComponent, DidUnequipEvent>(OnUnequip);

        SubscribeLocalEvent<VisibleContrabandComponent, DidEquipHandEvent>(OnEquipHands);
        SubscribeLocalEvent<VisibleContrabandComponent, DidUnequipHandEvent>(OnUnequippHands);
        
        Subs.CVar(_configuration,GoobCVars.ContrabandIconsEnabled, value => _isEnabled = value);
    }
    public void ContrabandDetect(EntityUid intentory, VisibleContrabandComponent component, SlotFlags slotFlags = SlotFlags.WITHOUT_POCKET)
    {
        if (!_isEnabled)
            return;
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
        ContrabandDetect(args.User, component, SlotFlags.NONE);
    }
    private void OnEquipHands(EntityUid uid, VisibleContrabandComponent component, DidEquipHandEvent args)
    {
        ContrabandDetect(args.User, component, SlotFlags.NONE);
    }
}
