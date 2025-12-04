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
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Security.ContrabandIcons;

public sealed class ContrabandIconsSystem : SharedContrabandIconsSystem
{
    [Dependency] private readonly IConfigurationManager _configuration = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedContrabandDetectorSystem _detectorSystem = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    private bool _isEnabled = true;
    private EntityQuery<ContrabandComponent> _contrabandQuery;
    private readonly HashSet<EntityUid> _queue = new();
    private EntityQuery<VisibleContrabandComponent> _visibleContrabandQuery;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        if (!_isEnabled)
            return;
        if (_queue.Count == 0)
            return;
        foreach (var personToMark in _queue)
        {
            if (_visibleContrabandQuery.TryComp(personToMark, out var comp) && comp.FirstItemTime is not null)
            {
                if (comp.VisibleItems.Count == 0)
                {
                    comp.StatusIcon = StatusToIcon(ContrabandStatus.None);
                    Dirty(personToMark, comp);
                    _queue.Remove(personToMark);
                    continue;
                }

                if (comp.FirstItemTime < _timing.CurTime)
                {
                    comp.StatusIcon = StatusToIcon(ContrabandStatus.Contraband);
                    Dirty(personToMark, comp);
                    _queue.Remove(personToMark);
                }
            }
        }
    }

    public override void Initialize()
    {
        base.Initialize();
        _contrabandQuery = GetEntityQuery<ContrabandComponent>();
        _visibleContrabandQuery = GetEntityQuery<VisibleContrabandComponent>();
        Subs.CVar(_configuration, GoobCVars.ContrabandIconsEnabled, value => _isEnabled = value);
        if (_isEnabled)
        {
            SubscribeLocalEvent<VisibleContrabandComponent, DidEquipEvent>(OnEquip);
            SubscribeLocalEvent<VisibleContrabandComponent, DidUnequipEvent>(OnUnequip);

            SubscribeLocalEvent<VisibleContrabandComponent, DidEquipHandEvent>(OnEquipHands);
            SubscribeLocalEvent<VisibleContrabandComponent, DidUnequipHandEvent>(OnUnequipHands);

            SubscribeLocalEvent<IdCardInsertedEvent>(OnIdCardInserted);
            SubscribeLocalEvent<IdCardRemovedEvent>(OnIdCardRemoved);
        }
    }

    private void OnEquip(EntityUid uid, VisibleContrabandComponent comp, DidEquipEvent args)
    {
        var createdTime = _timing.TickPeriod.Mul(MetaData(args.Equipee).CreationTick.Value); // time entity was created
        if (!_isEnabled ||
            !_contrabandQuery.TryComp(args.Equipment, out var contra) ||
            _detectorSystem.CheckContrabandPermission(args.Equipment, args.Equipee, contra) ||
            !(_timing.CurTime > createdTime + TimeSpan.FromSeconds(2f)) || args.SlotFlags == SlotFlags.POCKET)
            return;

        comp.VisibleItems.Add(args.Equipment);
        var newStatus = StatusToIcon(ContrabandStatus.Contraband);
        if (comp.StatusIcon != newStatus)
        {
            comp.StatusIcon = newStatus;
            Dirty(args.Equipee, comp);
        }
    }

    private void OnEquipHands(EntityUid uid, VisibleContrabandComponent comp, DidEquipHandEvent args)
    {
        var createdTime = _timing.TickPeriod.Mul(MetaData(args.User).CreationTick.Value);
        if (!_isEnabled ||
            !_contrabandQuery.TryComp(args.Equipped, out var contra) ||
            _detectorSystem.CheckContrabandPermission(args.Equipped, args.User, contra) ||
            !(_timing.CurTime > createdTime + TimeSpan.FromSeconds(2f)))
            return;
        comp.VisibleItems.Add(args.Equipped);
        if (comp.VisibleItems.Count <= 1)
        {
            _queue.Add(args.User);
            comp.FirstItemTime = _timing.CurTime + comp.VisibleTimeout;
        }
        else
        {
            var newStatus = StatusToIcon(ContrabandStatus.Contraband);
            if (comp.StatusIcon != newStatus)
            {
                comp.StatusIcon = newStatus;
                Dirty(args.User, comp);
            }
        }
    }

    private void OnUnequipHands(EntityUid uid, VisibleContrabandComponent comp, DidUnequipHandEvent args)
    {
        comp.VisibleItems.Remove(args.Unequipped);
        if (comp.VisibleItems.Count == 0)
        {
            var newStatus = StatusToIcon(ContrabandStatus.None);
            if (comp.StatusIcon != newStatus)
            {
                comp.StatusIcon = newStatus;
                Dirty(args.User, comp);
            }
        }
    }

    private void OnUnequip(EntityUid uid, VisibleContrabandComponent comp, DidUnequipEvent args)
    {
        comp.VisibleItems.Remove(args.Equipment);
        if (comp.VisibleItems.Count == 0)
        {
            var newStatus = StatusToIcon(ContrabandStatus.None);
            if (comp.StatusIcon != newStatus)
            {
                comp.StatusIcon = newStatus;
                Dirty(args.Equipee, comp);
            }
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

    /// <summary>
    ///     Handler for identity changes. When an entity's identity changes, re-evaluate their visible items
    ///     and update the contraband status icon if necessary.
    /// </summary>
    private void OnIdCardInserted(IdCardInsertedEvent args)
    {
        var uid = args.TargetUid;
        CheckAllContraOfContainerOwner(uid);
    }

    private void OnIdCardRemoved(IdCardRemovedEvent args)
    {
        var uid = args.TargetUid;
        CheckAllContraOfContainerOwner(uid);
    }

    private void CheckAllContraOfContainerOwner(EntityUid uid)
    {
        uid = GetHighestContainerOwner(uid);

        if (!_visibleContrabandQuery.TryComp(uid, out var visible))
            return;
        var contralist = _detectorSystem.FindContraband(uid, false, SlotFlags.WITHOUT_POCKET);
        var newStatus = StatusToIcon(contralist.Count > 0 ? ContrabandStatus.Contraband : ContrabandStatus.None);
        if (visible.StatusIcon != newStatus)
        {
            visible.StatusIcon = newStatus;
            Dirty(uid, visible);
        }
    }

    public EntityUid GetHighestContainerOwner(EntityUid uid)
    {
        while (_inventory.TryGetContainingEntity(uid, out var containerEntity))
        {
            if (containerEntity is { } owner)
                uid = owner;
            else
                break;
        }

        return uid;
    }
}