using System.Linq;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Shared.Contraband;
using Content.Goobstation.Shared.Security.ContrabandIcons;
using Content.Goobstation.Shared.Security.ContrabandIcons.Components;
using Content.Shared.Contraband;
using Content.Shared.Hands;
using Content.Shared.Inventory.Events;
using Content.Shared.Strip.Components;
using Robust.Shared.Configuration;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Security.ContrabandIcons;

public sealed class ContrabandIconsSystem : SharedContrabandIconsSystem
{
    [Dependency] private readonly IConfigurationManager _configuration = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedContrabandDetectorSystem _detectorSystem = default!;
    private bool _isEnabled = true;
    private TimeSpan _nextUpdate;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        if (!_isEnabled)
            return;
        if (_timing.CurTime < _nextUpdate)
            return;
        CheckVisibleContra();
        _nextUpdate = _timing.CurTime + TimeSpan.FromMilliseconds(1000);
    }

    public override void Initialize()
    {
        base.Initialize();
        _nextUpdate = TimeSpan.Zero;
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
        var thiefbuff = 1;
        if (HasComp<ThievingComponent>(args.Equipee))
            thiefbuff = 2;
        if (HasComp<ContrabandComponent>(args.Equipment))
            comp.VisibleItems.TryAdd(args.Equipment, _timing.CurTime + (comp.VisibleTimeout * thiefbuff));
    }

    private void OnEquipHands(EntityUid uid, VisibleContrabandComponent comp, DidEquipHandEvent args)
    {
        var thiefbuff = 1;
        if (HasComp<ThievingComponent>(args.User))
            thiefbuff = 2;
        if (HasComp<ContrabandComponent>(args.Equipped))
            comp.VisibleItems.TryAdd(args.Equipped, _timing.CurTime + (comp.VisibleTimeout * thiefbuff));
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

    private void CheckVisibleContra()
    {
        var query = EntityQueryEnumerator<VisibleContrabandComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            bool timeSpanDone;
            if (comp.VisibleItems.Count != 0 && comp.VisibleItems.Values.Any(value => value < _timing.CurTime))
                timeSpanDone = true;
            else
                continue;
            var newStatus = StatusToIcon(timeSpanDone && CheckItemsInComponent(comp, uid)
                ? ContrabandStatus.Contraband
                : ContrabandStatus.None);
            if (comp.StatusIcon != newStatus)
            {
                comp.StatusIcon = newStatus;
                Dirty(uid, comp);
            }
        }

        query.Dispose();
    }

    private bool CheckItemsInComponent(VisibleContrabandComponent component, EntityUid owner)
    {
        var returnValue = false;
        foreach (var item in component.VisibleItems.Keys)
        {
            var contra = Comp<ContrabandComponent>(item);
            if (!_detectorSystem.CheckContrabandPermission(item, owner, contra))
                returnValue = true;
        }

        return returnValue;
    }
}