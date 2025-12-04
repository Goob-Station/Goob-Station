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
    #region Dependencies
    [Dependency] private readonly IConfigurationManager _configuration = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedContrabandDetectorSystem _detectorSystem = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    private bool _isEnabled = true;
    private EntityQuery<ContrabandComponent> _contrabandQuery;
    private readonly HashSet<EntityUid> _queue = new();
    private EntityQuery<VisibleContrabandComponent> _visibleContrabandQuery;
    #endregion Dependencies
    /// <summary>
    /// Updates the contraband status icons for entities in the queue based on the timing of their first visible contraband
    /// item.
    /// </summary>
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

    #region EventHandlers

    // <summary>
    // on item equip check for contraband and permissions for that contraband
    // if contraband and no permission add to visible list and update icon
    // </summary>
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

    // <summary>
    // same as above with exception that if you take someone it takes time to actually show up
    // </summary>
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

    // <summary>
    // triggered on unequip from hands
    // </summary>
    private void OnUnequipHands(EntityUid uid, VisibleContrabandComponent comp, DidUnequipHandEvent args)
    {
        UnequipContra(comp, args.User, args.Unequipped);
    }

    // <summary>
    // triggered on unequip from inventory
    // </summary>
    private void OnUnequip(EntityUid uid, VisibleContrabandComponent comp, DidUnequipEvent args)
    {
        UnequipContra(comp, args.Equipee, args.Equipment);
    }

    /// <summary>
    ///     Handles ID card insertions and removals.
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

    #endregion

    #region Helpers

    /// <summary>
    ///     returns the icon string based on status enum
    /// </summary>
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
    ///     Checks all contraband items for the highest-level container owner of the given entity and updates their visible
    ///     contraband status icon accordingly.
    /// </summary>
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

    /// <summary>
    ///     Recursively gets the highest-level container owner of the given entity.
    /// </summary>
    /// <param name="uid">uid of the container</param>
    /// <returns>EntityUid of the highest level entity</returns>
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

    // <summary>
    // Remove item from visible list and if list is empty update icon to none
    // </summary>
    private void UnequipContra(VisibleContrabandComponent comp, EntityUid unequipper, EntityUid unequipped)
    {
        comp.VisibleItems.Remove(unequipped);
        if (comp.VisibleItems.Count != 0)
            return;
        var newStatus = StatusToIcon(ContrabandStatus.None);
        if (comp.StatusIcon == newStatus)
            return;
        comp.StatusIcon = newStatus;
        Dirty(unequipper, comp);
    }

    #endregion
}