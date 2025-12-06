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
        if (!_isEnabled)
            return;
        if (args.SlotFlags == SlotFlags.IDCARD)
        {
            CheckAllContra(args.Equipee);
            return;
        }
        if (args.SlotFlags == SlotFlags.POCKET)
            return;
        if (!_contrabandQuery.TryComp(args.Equipment, out var contra))
            return;
        if (contra.Severity == MinorSeverity || contra.Severity == Honkraband)
            return;
        var hasPermission = _detectorSystem.CheckContrabandPermission(args.Equipment, args.Equipee, contra);
        if ((contra.Severity == RestrictedSeverity || contra.Severity == GrandTheftSeverity) && hasPermission)
            return;
        comp.VisibleItems.Add(args.Equipment);
        UpdateStatusIcon(comp, args.Equipee, ContrabandStatus.Contraband);
    }

    private void OnUnequip(EntityUid uid, VisibleContrabandComponent comp, DidUnequipEvent args)
    {
        if (args.SlotFlags == SlotFlags.IDCARD)
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
        if (!_isEnabled)
            return;
        if (!_contrabandQuery.TryComp(args.Equipped, out var contra))
            return;
        if (contra.Severity == MinorSeverity)
            return;
        var hasPermission = _detectorSystem.CheckContrabandPermission(args.Equipped, args.User, contra);
        if ((contra.Severity == RestrictedSeverity || contra.Severity == GrandTheftSeverity) && hasPermission)
            return;
        UpdateStatusIcon(comp, args.User, ContrabandStatus.Contraband);
    }
    
    private void OnUnequipHands(EntityUid uid, VisibleContrabandComponent comp, DidUnequipHandEvent args)
    {
        if (comp.VisibleItems.Count == 0)
            UpdateStatusIcon(comp, args.User, ContrabandStatus.None);
    }
    private void OnIdCardInserted(IdCardInsertedEvent args)
    {
        CheckAllContra(args.TargetUid);
    }

    private void OnIdCardRemoved(IdCardRemovedEvent args)
    {
        CheckAllContra(args.TargetUid);
    }
    private void OnComponentStartup(EntityUid uid, VisibleContrabandComponent component, ComponentStartup args)
    {
        CheckAllContra(uid);
    }
}
