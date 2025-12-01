// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 BombasterDS <deniskaporoshok@gmail.com>
// SPDX-FileCopyrightText: 2025 BombasterDS2 <shvalovdenis.workmail@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Contraband;
using Content.Shared.Power;
using Robust.Shared.Timing;
using Content.Shared.Inventory;
using Content.Shared.Storage;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Power.EntitySystems;
using System.Linq;
using Robust.Shared.Prototypes;
using Content.Shared.Roles;
using Content.Shared.Access.Systems;

namespace Content.Goobstation.Shared.Contraband;

public abstract class SharedContrabandDetectorSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly ContrabandSystem _contrabandSystem = default!;
    [Dependency] private readonly SharedIdCardSystem _idCardSystem = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _powerReceiverSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeMan = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ContrabandDetectorComponent, PowerChangedEvent>(OnPowerChange);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<ContrabandDetectorComponent>();
        while (query.MoveNext(out var uid, out var detector))
        {
            if (detector.State != ContrabandDetectorState.Powered
                && detector.LastScanTime + detector.ScanTimeOut < _timing.CurTime
                && _powerReceiverSystem.IsPowered(uid))
            {
                detector.State = ContrabandDetectorState.Powered;
                UpdateVisuals((uid, detector));
                Dirty(uid, detector);
            }

            if (detector.Scanned.Count == 0)// go to next if there are no scanned
                continue;

            var keysToRemove = new List<EntityUid>(detector.Scanned.Count);
            foreach (var scan in detector.Scanned)
            {
                if (_timing.CurTime > scan.Value)
                    keysToRemove.Add(scan.Key);
            }
            foreach (var key in keysToRemove)
            {
                detector.Scanned.Remove(key);
            }
            if (keysToRemove.Count > 0)
                detector.Scanned.TrimExcess();
        }
    }
    public bool IsContraband(EntityUid uid)
    {
        if (HasComp<ContrabandComponent>(uid) && !HasComp<UndetectableContrabandComponent>(uid))
            return true;

        return false;
    }

    public List<EntityUid> FindContraband(EntityUid uid)
    {
        List<EntityUid> listOfContraband = new();
        List<EntityUid> itemsToCheck = new();

        itemsToCheck.Add(uid);

        // Check items in inner storage
        itemsToCheck.AddRange(RecursiveFindInStorage(uid));

        // Check items in inventory slots and storages
        var enumerator = _inventorySystem.GetSlotEnumerator(uid);
        while (enumerator.MoveNext(out var slot))
        {
            var item = slot.ContainedEntity;

            if (item == null)
                continue;

            itemsToCheck.Add(item.Value);
            itemsToCheck.AddRange(RecursiveFindInStorage(item.Value));
        }

        // Check items in hands
        var handEnumerator = _handsSystem.EnumerateHeld(uid);
        foreach (var handItem in handEnumerator)
        {
            itemsToCheck.Add(handItem);
            itemsToCheck.AddRange(RecursiveFindInStorage(handItem));
        }

        foreach (var item in itemsToCheck)
        {
            if (IsContraband(item) && !CheckContrabandPermission(item, uid))
                listOfContraband.Add(item);
        }

        return listOfContraband;
    }

    /// <summary>
    /// Check items with storage component (like bags) to prevent check in itemSlots, implants.
    /// </summary>
    /// <param name="uid"></param>
    /// <returns></returns>
    private List<EntityUid> RecursiveFindInStorage(EntityUid uid, HashSet<EntityUid>? visited = null)
    {
        visited ??= new HashSet<EntityUid>();
        List<EntityUid> listToCheck = new();

        // Prevents rechecking same entity
        if (!visited.Add(uid))
            return listToCheck;

        if (!TryComp<StorageComponent>(uid, out var storage)
            || HasComp<HideContrabandContentComponent>(uid)
            || storage.Container.ContainedEntities.Count == 0)
            return listToCheck;

        foreach (var item in storage.Container.ContainedEntities)
        {
            listToCheck.Add(item);
            listToCheck.AddRange(RecursiveFindInStorage(item, visited));
        }

        return listToCheck;
    }

    protected void UpdateVisuals(Entity<ContrabandDetectorComponent> detector)
    {
        _appearanceSystem.SetData(detector, ContrabandDetectorVisuals.VisualState, detector.Comp.State);
    }

    private void OnPowerChange(Entity<ContrabandDetectorComponent> detector, ref PowerChangedEvent args)
    {
        if (!args.Powered)
            detector.Comp.State = ContrabandDetectorState.Off;
        else
            detector.Comp.State = ContrabandDetectorState.Powered;

        UpdateVisuals(detector);
        Dirty(detector);
    }

    public void ChangeFalseDetectionChance(Entity<ContrabandDetectorComponent> detector, float multiplier)
    {
        var comp = detector.Comp;

        if (comp.IsFalseDetectingChanged)
            comp.FalseDetectingChance /= multiplier;
        else
            comp.FalseDetectingChance *= multiplier;

        comp.IsFalseDetectingChanged = !comp.IsFalseDetectingChanged;

        Dirty(detector);
    }

    public void TurnFakeScanning(Entity<ContrabandDetectorComponent> detector)
    {
        var comp = detector.Comp;

        comp.IsFalseScanning = !comp.IsFalseScanning;

        Dirty(detector);
    }

    /// <summary>
    /// Checks permission for user to have contraband. 
    /// </summary>
    /// <param name="contraband"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public bool CheckContrabandPermission(EntityUid contraband, EntityUid user, ContrabandComponent? component = null)
    {
        // No contraband = have permission 
        if (!Resolve(contraband, ref component))
            return true;

        var jobs = component.AllowedJobs.Select(p => _prototypeMan.Index(p).LocalizedName).ToArray();

        var job = "";
        List<ProtoId<DepartmentPrototype>> departments = new();
        if (_idCardSystem.TryFindIdCard(user, out var id))
        {
            departments = id.Comp.JobDepartments;
            if (id.Comp.LocalizedJobTitle is not null)
            {
                job = id.Comp.LocalizedJobTitle;
            }
        }

        if (departments.Intersect(component.AllowedDepartments).Any() || jobs.Contains(job))
            return true;

        return false;
    }
}