using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Common.Security.ContrabandIcons.Events;
using Content.Goobstation.Shared.Contraband;
using Content.Goobstation.Shared.Security.ContrabandIcons.Components;
using Content.Shared.Contraband;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Security.ContrabandIcons;

public abstract class SharedContrabandIconsSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _configuration = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedContrabandDetectorSystem _detectorSystem = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;

    private bool _isEnabled = true;
    private EntityQuery<ContrabandComponent> _contrabandQuery;
    private EntityQuery<VisibleContrabandComponent> _visibleContrabandQuery;

    public override void Initialize()
    {
        base.Initialize();
        _contrabandQuery = GetEntityQuery<ContrabandComponent>();
        _visibleContrabandQuery = GetEntityQuery<VisibleContrabandComponent>();

        Subs.CVar(_configuration, GoobCVars.ContrabandIconsEnabled, value => _isEnabled = value);

        SubscribeLocalEvent<VisibleContrabandComponent, DidEquipEvent>(OnEquip);
        SubscribeLocalEvent<VisibleContrabandComponent, DidUnequipEvent>(OnUnequip);

        SubscribeLocalEvent<IdCardInsertedEvent>(OnIdCardInserted);
        SubscribeLocalEvent<IdCardRemovedEvent>(OnIdCardRemoved);
    }

    private void OnEquip(EntityUid uid, VisibleContrabandComponent comp, DidEquipEvent args)
    {
        if (!_isEnabled ||
            !_contrabandQuery.TryComp(args.Equipment, out var contra) 
            || (contra.Severity == "Restricted" && _detectorSystem.CheckContrabandPermission(args.Equipment, args.Equipee, contra)) 
            || contra.Severity == "Minor" 
            || contra.Severity == "GrandTheft" 
            || args.SlotFlags == SlotFlags.POCKET)
            return;

        comp.VisibleItems.Add(args.Equipment);
        UpdateStatusIcon(comp, args.Equipee, ContrabandStatus.Contraband);
    }

    private void OnUnequip(EntityUid uid, VisibleContrabandComponent comp, DidUnequipEvent args)
    {
        comp.VisibleItems.Remove(args.Equipment);
        if (comp.VisibleItems.Count == 0)
            UpdateStatusIcon(comp, args.Equipee, ContrabandStatus.None);
    }

    private void OnIdCardInserted(IdCardInsertedEvent args)
    {
        CheckAllContra(args.TargetUid);
    }

    private void OnIdCardRemoved(IdCardRemovedEvent args)
    {
        CheckAllContra(args.TargetUid);
    }

    private string StatusToIcon(ContrabandStatus status)
    {
        return status == ContrabandStatus.Contraband ? "ContrabandIconContraband" : "ContrabandIconNone";
    }

    private void CheckAllContra(EntityUid uid)
    {
        uid = GetHighestContainerOwner(uid);

        if (!_visibleContrabandQuery.TryComp(uid, out var visible))
            return;

        var contralist = _detectorSystem.FindContraband(uid, false, SlotFlags.WITHOUT_POCKET);
        var status = contralist.Count > 0 ? ContrabandStatus.Contraband : ContrabandStatus.None;
        UpdateStatusIcon(visible, uid, status);
    }

    private void UpdateStatusIcon(VisibleContrabandComponent comp, EntityUid uid, ContrabandStatus status)
    {
        var newStatus = StatusToIcon(status);
        if (comp.StatusIcon == newStatus)
            return;

        comp.StatusIcon = newStatus;
        Dirty(uid, comp);
    }

    public EntityUid GetHighestContainerOwner(EntityUid uid)
    {
        while (_inventory.TryGetContainingEntity(uid, out var containerEntity))
        {
            uid = containerEntity.Value;
        }

        return uid;
    }
}