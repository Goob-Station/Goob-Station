using System.Linq;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Shared.Contraband;
using Content.Goobstation.Shared.Security.ContrabandIcons;
using Content.Goobstation.Shared.Security.ContrabandIcons.Components;
using Content.Shared.Access.Components;
using Content.Shared.Contraband;
using Content.Shared.Hands;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Strip.Components;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Security.ContrabandIcons;

public sealed class ContrabandIconsSystem : SharedContrabandIconsSystem
{
    [Dependency] private readonly IConfigurationManager _configuration = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedContrabandDetectorSystem _detectorSystem = default!;
    private bool _isEnabled = true;
    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        if (!_isEnabled)
            return;
        var query = EntityQueryEnumerator<VisibleContrabandComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            var timeSpanDone = false;
            var contraCheck = false;
            if (comp.VisibleItems.Count != 0 && comp.VisibleItems.Values.All(value => value < _timing.CurTime))
                timeSpanDone = true;
            if (!timeSpanDone)
                continue;
            if (CheckItemsInComponent(comp, uid))
                contraCheck = true;
            comp.StatusIcon = StatusToIcon((timeSpanDone && contraCheck) ? ContrabandStatus.Contraband : ContrabandStatus.None);
            Dirty(uid, comp);
        }
        query.Dispose();
    }

    public override void Initialize()
    {
        base.Initialize();
        Subs.CVar(_configuration, GoobCVars.ContrabandIconsEnabled, value => _isEnabled = value);
        if (_isEnabled)
        {
            //SubscribeLocalEvent<VisibleContrabandComponent, MapInitEvent>(OnMapInit);

            SubscribeLocalEvent<VisibleContrabandComponent, DidEquipEvent>(OnEquip);
            SubscribeLocalEvent<VisibleContrabandComponent, DidUnequipEvent>(OnUnequip);

            SubscribeLocalEvent<VisibleContrabandComponent, DidEquipHandEvent>(OnEquipHands);
            SubscribeLocalEvent<VisibleContrabandComponent, DidUnequipHandEvent>(OnUnequipHands);
        }
    }

    private void OnEquip(EntityUid uid, VisibleContrabandComponent comp, DidEquipEvent args)
    {
        comp.VisibleItems.TryAdd(args.Equipment, _timing.CurTime + comp.VisibleTimeout);
    }
    
    private void OnEquipHands(EntityUid uid, VisibleContrabandComponent comp, DidEquipHandEvent args)
    {
        comp.VisibleItems.TryAdd(args.Equipped, _timing.CurTime + comp.VisibleTimeout);
    }

    private void OnUnequipHands(EntityUid uid, VisibleContrabandComponent comp, DidUnequipHandEvent args)
    {
        comp.VisibleItems.Remove(args.Unequipped);
    }

    private void OnUnequip(EntityUid uid, VisibleContrabandComponent comp, DidUnequipEvent args)
    {
        comp.VisibleItems.Remove(args.Equipment);
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
    private bool CheckItemsInComponent(VisibleContrabandComponent component, EntityUid owner)
    {
        foreach (var item in component.VisibleItems.Keys)
        {
            if(!TryComp<ContrabandComponent>(item, out var contraband))
                continue;
            if (_detectorSystem.IsContraband(item) && !_detectorSystem.CheckContrabandPermission(item, owner, contraband))
                return true;
            else
                return false;
        }
        return false;
    }
}
