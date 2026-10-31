using Content.Goobstation.Common.Nuke;
using Content.Server.Mind;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Destructible;
using Content.Shared.Nuke;
using Content.Shared.Roles;

namespace Content.Server.Nuke;

public sealed partial class NukeSystem
{
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly SharedRoleSystem _roles = default!;

    private void OnDiskInsertAttempt(EntityUid uid, NukeComponent component, ref ItemSlotInsertAttemptEvent args)
    {
        if (args.Slot.ID != component.DiskSlot.ID)
            return;

        if (!HasComp<NukeDiskComponent>(args.Item))
            return;

        component.LastDiskInserter = args.User;
    }

    private bool IsAntagonist(EntityUid? user)
    {
        if (user is not { } userUid || !_mind.TryGetMind(userUid, out var mindId, out _))
            return false;

        return _roles.MindIsAntagonist(mindId);
    }

    private void GoobRaiseDisarmed(EntityUid nuke, EntityUid user)
    {
        var ev = new NukeDisarmedByEvent(nuke);
        RaiseLocalEvent(user, ref ev);
    }

    private void OnNukeDestroyed(EntityUid uid, NukeComponent component, DestructionEventArgs args)
    {
        var stationUid = _station.GetOwningStation(uid);
        if (stationUid != null && component.Status == NukeStatus.ARMED)
            _alertLevel.SetLevel(stationUid.Value, component.AlertLevelOnDeactivate, true, true, true);

        // Goobstation
        var coords = _transform.GetMoverCoordinates(uid);
        var wreck = Spawn(component.WreckPrototype, coords);
        EnsureComp<NukeWreckCodeComponent>(wreck).Code = component.Code;

        SilentlyDeactivate(uid, component);

        // Goobstation
        if (component.DiskSlot.HasItem)
        {
            _itemSlots.SetLock(uid, component.DiskSlot, false);
            _itemSlots.TryEject(uid, component.DiskSlot, null, out _, doAfter: false);
        }
    }
}
