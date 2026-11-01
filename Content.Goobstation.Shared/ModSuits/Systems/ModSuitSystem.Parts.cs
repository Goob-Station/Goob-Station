using Content.Goobstation.Shared.ModSuits.Components;
using Content.Shared.Inventory.Events;

namespace Content.Goobstation.Shared.ModSuits.Systems;

public sealed partial class ModSuitSystem
{
    private void SpawnParts(Entity<ModSuitComponent> ent)
    {
        if (ent.Comp.PartsContainer.ContainedEntities.Count != 0)
            return;

        foreach (var (slot, proto) in ent.Comp.Parts)
        {
            if (!PredictedTrySpawnInContainer(proto, ent, ent.Comp.PartsContainerId, out var part))
                continue;

            var partComp = EnsureComp<ModPartComponent>(part.Value);
            partComp.Suit = ent;
            partComp.Slot = slot;
            Dirty(part.Value, partComp);

            ent.Comp.PartEntities[slot] = part.Value;
        }
    }

    public bool IsPartDeployed(Entity<ModSuitComponent> ent, string slot)
    {
        return ent.Comp.PartEntities.TryGetValue(slot, out var part)
               && !ent.Comp.PartsContainer.Contains(part);
    }

    public bool AllPartsDeployed(Entity<ModSuitComponent> ent)
    {
        if (ent.Comp.Wearer is not { } wearer)
            return false;

        foreach (var (slot, part) in ent.Comp.PartEntities)
        {
            if (!_inventory.TryGetSlotEntity(wearer, slot, out var worn) || worn != part)
                return false;
        }

        return true;
    }

    public void ToggleAllParts(Entity<ModSuitComponent> ent, EntityUid user)
    {
        if (ent.Comp.Wearer == null || ent.Comp.DeployQueue.Count > 0)
            return;

        if (ent.Comp.State != ModSuitState.Inactive)
        {
            PopupFail(ent, user, "modsuit-deactivate-first");
            return;
        }

        var retracting = true;
        foreach (var slot in ent.Comp.PartEntities.Keys)
        {
            if (!IsPartDeployed(ent, slot))
                retracting = false;
        }

        var queue = new List<string>();
        foreach (var slot in ent.Comp.PartEntities.Keys)
        {
            if (IsPartDeployed(ent, slot) == retracting)
                queue.Add(slot);
        }

        if (retracting)
            queue.Reverse();

        ent.Comp.DeployQueue = queue;
        ent.Comp.DeployQueueRetracting = retracting;
        ent.Comp.NextDeployStep = _timing.CurTime;
        Dirty(ent);
        RaiseUpdated(ent);
    }

    private void UpdateDeployQueues()
    {
        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<ModSuitComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.DeployQueue.Count == 0 || now < comp.NextDeployStep || !ShouldStep(comp))
                continue;

            var ent = (uid, comp);
            if (comp.Wearer is not { } wearer || comp.State != ModSuitState.Inactive)
            {
                comp.DeployQueue.Clear();
                Dirty(uid, comp);
                continue;
            }

            var slot = comp.DeployQueue[0];
            comp.DeployQueue.RemoveAt(0);

            var success = comp.DeployQueueRetracting
                ? TryRetractPart(ent, slot, wearer)
                : TryDeployPart(ent, slot, wearer);

            if (!success)
            {
                comp.DeployQueue.Clear();
                if (!comp.DeployQueueRetracting)
                    _popup.PopupClient(Loc.GetString("modsuit-deploy-failed"), wearer, wearer);
            }

            comp.NextDeployStep = now + comp.DeployStepTime;
            Dirty(uid, comp);
            RaiseUpdated(uid);
        }
    }

    public void TogglePart(Entity<ModSuitComponent> ent, string slot, EntityUid user)
    {
        if (IsPartDeployed(ent, slot))
            TryRetractPart(ent, slot, user);
        else
            TryDeployPart(ent, slot, user);
    }

    public bool TryDeployPart(Entity<ModSuitComponent> ent, string slot, EntityUid? user, bool silent = false)
    {
        if (ent.Comp.Wearer is not { } wearer
            || !ent.Comp.PartEntities.TryGetValue(slot, out var part)
            || !ent.Comp.PartsContainer.Contains(part)
            || !TryComp<ModPartComponent>(part, out var partComp))
            return false;

        if (ent.Comp.State != ModSuitState.Inactive)
        {
            PopupFail(ent, user, "modsuit-deactivate-first");
            return false;
        }

        if (_inventory.TryGetSlotEntity(wearer, slot, out var current))
        {
            if (partComp.StoredContainer.ContainedEntity != null
                || !_inventory.TryUnequip(wearer, wearer, slot, out var removed, silent: true, predicted: true)
                || !_container.Insert(removed.Value, partComp.StoredContainer))
            {
                if (user != null && !silent)
                    _popup.PopupClient(Loc.GetString("modsuit-part-slot-occupied", ("item", current.Value)), ent, user.Value);
                return false;
            }
        }

        _movingParts.Add(part);
        var equipped = _inventory.TryEquip(wearer, wearer, part, slot, silent: silent, force: true, predicted: true);
        _movingParts.Remove(part);

        if (!equipped)
        {
            RestoreStoredClothing((part, partComp), wearer, slot);
            return false;
        }

        Dirty(ent);
        return true;
    }

    public bool TryRetractPart(Entity<ModSuitComponent> ent, string slot, EntityUid? user, bool force = false, bool silent = false)
    {
        if (!ent.Comp.PartEntities.TryGetValue(slot, out var part)
            || ent.Comp.PartsContainer.Contains(part)
            || !TryComp<ModPartComponent>(part, out var partComp))
            return false;

        if (!force && ent.Comp.State != ModSuitState.Inactive)
        {
            PopupFail(ent, user, "modsuit-deactivate-first");
            return false;
        }

        if (partComp.Sealed)
            SetPartSealed((part, partComp), ent, false, true);

        var wearer = ent.Comp.Wearer;
        _movingParts.Add(part);
        if (wearer != null && _inventory.TryGetSlotEntity(wearer.Value, slot, out var worn) && worn == part)
            _inventory.TryUnequip(wearer.Value, wearer.Value, slot, silent: silent, force: true, predicted: true);
        _container.Insert(part, ent.Comp.PartsContainer);
        _movingParts.Remove(part);

        if (wearer != null)
            RestoreStoredClothing((part, partComp), wearer.Value, slot);
        else
            DropStoredClothing((part, partComp));

        Dirty(ent);
        return true;
    }

    public void CopyDeployState(Entity<ModSuitComponent?> source, Entity<ModSuitComponent?> target)
    {
        if (!Resolve(source, ref source.Comp, false) || !Resolve(target, ref target.Comp, false))
            return;

        var targetEnt = (target.Owner, target.Comp);
        foreach (var slot in source.Comp.PartEntities.Keys)
        {
            if (IsPartDeployed((source.Owner, source.Comp), slot))
                TryDeployPart(targetEnt, slot, null, true);
        }

        if (source.Comp.State == ModSuitState.Active)
            ForceActivate(targetEnt);
    }

    private void RetractAllParts(Entity<ModSuitComponent> ent, EntityUid? wearer)
    {
        foreach (var slot in ent.Comp.PartEntities.Keys)
        {
            TryRetractPart(ent, slot, null, true, true);
        }
    }

    private void RestoreStoredClothing(Entity<ModPartComponent> part, EntityUid wearer, string slot)
    {
        if (part.Comp.StoredContainer.ContainedEntity is not { } stored)
            return;

        _container.Remove(stored, part.Comp.StoredContainer);
        if (!_inventory.TryEquip(wearer, wearer, stored, slot, silent: true, force: true, predicted: true))
            _hands.PickupOrDrop(wearer, stored);
    }

    private void DropStoredClothing(Entity<ModPartComponent> part)
    {
        if (part.Comp.StoredContainer.ContainedEntity is { } stored)
            _container.Remove(stored, part.Comp.StoredContainer);
    }

    [SubscribeLocalEvent]
    private void OnPartUnequipAttempt(Entity<ModPartComponent> ent, ref BeingUnequippedAttemptEvent args)
    {
        if (_movingParts.Contains(ent))
            return;

        args.Reason = ent.Comp.Sealed ? "modsuit-part-sealed" : "modsuit-part-retract-instead";
        args.Cancel();
    }

    [SubscribeLocalEvent]
    private void OnPartGotUnequipped(Entity<ModPartComponent> ent, ref GotUnequippedEvent args)
    {
        if (_timing.ApplyingState
            || _movingParts.Contains(ent)
            || _toggleableClothing.IsInventoryTransferring(args.Equipee)
            || ent.Comp.Suit is not { } suitUid
            || TerminatingOrDeleted(suitUid)
            || !TryComp<ModSuitComponent>(suitUid, out var suit))
            return;

        var suitEnt = (suitUid, suit);
        if (suit.State != ModSuitState.Inactive)
            BreakSeal(suitEnt);

        if (ent.Comp.Sealed)
            SetPartSealed(ent, suitEnt, false, true);

        _container.Insert(ent.Owner, suit.PartsContainer);
        RestoreStoredClothing(ent, args.Equipee, ent.Comp.Slot);
        Dirty(suitUid, suit);
    }
}
