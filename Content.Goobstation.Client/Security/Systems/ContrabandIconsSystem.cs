using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Common.Security.ContrabandIcons.Events;
using Content.Goobstation.Shared.Contraband;
using Content.Goobstation.Shared.Security.ContrabandIcons;
using Content.Goobstation.Shared.Security.ContrabandIcons.Components;
using Content.Shared.Contraband;
using Content.Shared.Hands;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Security.Systems;

public sealed class ContrabandIconsSystem : SharedContrabandIconsSystem
{
    [Dependency] private readonly IConfigurationManager _configuration = default!;
    [Dependency] private readonly SharedContrabandDetectorSystem _detectorSystem = default!;
    
    private bool _isEnabled = true;
    private EntityQuery<ContrabandComponent> _contrabandQuery;
    
    private static readonly ProtoId<ContrabandSeverityPrototype> MinorSeverity = "Minor";
    private static readonly ProtoId<ContrabandSeverityPrototype> RestrictedSeverity = "Restricted";
    private static readonly ProtoId<ContrabandSeverityPrototype> GrandTheftSeverity = "GrandTheft";
    private static readonly ProtoId<ContrabandSeverityPrototype> Honkraband = "Honkraband";
    public override void Initialize()
    {
        base.Initialize();
        _contrabandQuery = GetEntityQuery<ContrabandComponent>();

        Subs.CVar(_configuration, GoobCVars.ContrabandIconsEnabled, value => _isEnabled = value);

        SubscribeLocalEvent<VisibleContrabandComponent, DidEquipEvent>(OnEquip);
        SubscribeLocalEvent<VisibleContrabandComponent, DidUnequipEvent>(OnUnequip);
        
        SubscribeLocalEvent<VisibleContrabandComponent, DidEquipHandEvent>(OnEquipHands);
        SubscribeLocalEvent<VisibleContrabandComponent, DidUnequipHandEvent>(OnUnequipHands);

        SubscribeLocalEvent<IdCardInsertedEvent>(OnIdCardInserted);
        SubscribeLocalEvent<IdCardRemovedEvent>(OnIdCardRemoved);
        SubscribeLocalEvent<VisibleContrabandComponent, ComponentStartup>(OnComponentStartup);
    }

    private void OnEquip(EntityUid uid, VisibleContrabandComponent comp, DidEquipEvent args)
    {
        if (!_isEnabled || !MetaData(args.Equipee).EntityInitialized) // stupid fucking equip event during intialization breaks ID acquisition
            return;
        if (args.SlotFlags == SlotFlags.IDCARD) // when something happens with the pda slot we need to recheck everything
        {
            CheckAllContra(args.Equipee);
            return;
        }
        if (IsNotContra(args.Equipment, args.Equipee) || args.SlotFlags == SlotFlags.POCKET)
            return;
        comp.VisibleItems.Add(args.Equipment);
        UpdateStatusIcon(comp, args.Equipee, ContrabandStatus.Contraband);
    }

    private void OnUnequip(EntityUid uid, VisibleContrabandComponent comp, DidUnequipEvent args)
    {
        if (args.SlotFlags == SlotFlags.IDCARD) // when something happens with the pda slot we need to recheck everything
        {
            CheckAllContra(args.Equipee);
            return;
        }
        comp.VisibleItems.Remove(args.Equipment);
        if (comp.VisibleItems.Count == 0)
            UpdateStatusIcon(comp, args.Equipee, ContrabandStatus.None);
    }

    private void OnEquipHands(EntityUid uid, VisibleContrabandComponent comp, DidEquipHandEvent args)
    {
        if (!_isEnabled || !MetaData(args.User).EntityInitialized) // stupid fucking hands event during intialization breaks ID acquisition
            return;
        if(IsNotContra(args.Equipped, args.User))
            return;
        UpdateStatusIcon(comp, args.User, ContrabandStatus.Contraband);
    }
    
    private void OnUnequipHands(EntityUid uid, VisibleContrabandComponent comp, DidUnequipHandEvent args)
    {
        if (comp.VisibleItems.Count == 0 && _isEnabled) 
            UpdateStatusIcon(comp, args.User, ContrabandStatus.None);
    }
    private void OnIdCardInserted(IdCardInsertedEvent args)
    {
        if(_isEnabled)
            CheckAllContra(args.TargetUid);
    }

    private void OnIdCardRemoved(IdCardRemovedEvent args)
    {
        if (_isEnabled)
            CheckAllContra(args.TargetUid);
    }
    private void OnComponentStartup(EntityUid uid, VisibleContrabandComponent component, ComponentStartup args)
    {
        if (_isEnabled)
            CheckAllContra(uid);
    }

    private bool IsNotContra(EntityUid item, EntityUid user)
    {
        return !_contrabandQuery.TryComp(item, out var contra) || (contra.Severity == MinorSeverity || contra.Severity == Honkraband || (contra.Severity == RestrictedSeverity || contra.Severity == GrandTheftSeverity)
            && _detectorSystem.CheckContrabandPermission(item, user, contra));
    }
}
