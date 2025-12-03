using System.Collections.Generic;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Shared.Contraband;
using Content.Goobstation.Shared.Security.ContrabandIcons;
using Content.Goobstation.Shared.Security.ContrabandIcons.Components;
using Content.Shared.Contraband;
using Content.Shared.Hands;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Security.ContrabandIcons;

public sealed class ContrabandIconsSystem : SharedContrabandIconsSystem
{
    [Dependency] private readonly IConfigurationManager _configuration = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedContrabandDetectorSystem _detectorSystem = default!;
    private bool _isEnabled = true;
    private Dictionary<EntityUid, TimeSpan> _pendingUpdate = new();

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        if (!_isEnabled)
            return;
        if (!(_pendingUpdate.Count > 0))
            return;
        var removeUpdated = new List<EntityUid>();
        foreach (var (uid, time) in _pendingUpdate)
        {
            if (time <= _timing.CurTime)
            {
                if (!TryComp<VisibleContrabandComponent>(uid, out var comp))
                {
                    removeUpdated.Add(uid);
                    continue;
                }
                DetectContraUpdateStatus(uid, comp);
                removeUpdated.Add(uid);
            }
        }

        foreach (var uid in removeUpdated)
        {
            _pendingUpdate.Remove(uid);
        }
    }

    public override void Initialize()
    {
        base.Initialize();
        Subs.CVar(_configuration, GoobCVars.ContrabandIconsEnabled, value => _isEnabled = value);
        if (_isEnabled)
        {
            SubscribeLocalEvent<VisibleContrabandComponent, DidEquipEvent>(OnEquip);
            SubscribeLocalEvent<VisibleContrabandComponent, DidUnequipEvent>(OnUnequip);

            SubscribeLocalEvent<VisibleContrabandComponent, DidEquipHandEvent>(OnEquipHands);
            SubscribeLocalEvent<VisibleContrabandComponent, DidUnequipHandEvent>(OnUnequipHands);
        }
    }

    private void OnEquip(EntityUid uid, VisibleContrabandComponent comp, DidEquipEvent args)
    {
        if(args.SlotFlags == SlotFlags.POCKET)
            return;
        if (HasComp<ContrabandComponent>(args.Equipment))
        {
            comp.VisibleItems.Add(args.Equipment);
            _pendingUpdate[uid] = _timing.CurTime + TimeSpan.FromMilliseconds(200);
        }
    }

    private void OnEquipHands(EntityUid uid, VisibleContrabandComponent comp, DidEquipHandEvent args)
    {
        if (HasComp<ContrabandComponent>(args.Equipped))
        {
            comp.VisibleItems.Add(args.Equipped);
            _pendingUpdate[uid] = _timing.CurTime + TimeSpan.FromMilliseconds(200);
        }
    }

    private void OnUnequipHands(EntityUid uid, VisibleContrabandComponent comp, DidUnequipHandEvent args)
    {
        comp.VisibleItems.Remove(args.Unequipped);
        _pendingUpdate[uid] = _timing.CurTime + TimeSpan.FromMilliseconds(200);
    }

    private void OnUnequip(EntityUid uid, VisibleContrabandComponent comp, DidUnequipEvent args)
    {
        comp.VisibleItems.Remove(args.Equipment);
        _pendingUpdate[uid] = _timing.CurTime + TimeSpan.FromMilliseconds(200);
    }

    private void DetectContraUpdateStatus(EntityUid uid, VisibleContrabandComponent comp)
    {
        bool hasContraband = false;
        foreach (var item in comp.VisibleItems)
        {
            var contra = Comp<ContrabandComponent>(item);
            if (!_detectorSystem.CheckContrabandPermission(item, uid, contra))
            {
                hasContraband = true;
                break;
            }
        }
        var newStatus = StatusToIcon(hasContraband ? ContrabandStatus.Contraband : ContrabandStatus.None);
        if (comp.StatusIcon != newStatus)
        {
            comp.StatusIcon = newStatus;
            Dirty(uid, comp);
        }
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
}
