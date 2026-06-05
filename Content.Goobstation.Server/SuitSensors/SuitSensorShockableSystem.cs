using Content.Server.Medical.SuitSensors;
using Content.Shared.Electrocution;
using Content.Shared.Inventory;
using Content.Shared.Medical.SuitSensor;
using Content.Shared.Popups;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.SuitSensors;

public sealed class SuitSensorShockableSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SuitSensorSystem _suitSensorSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ElectrocutedEvent>(OnElectrocuted);
    }

    private void OnElectrocuted(ElectrocutedEvent args)
    {
        var enumerator = _inventory.GetSlotEnumerator(args.TargetUid);
        var modes = Enum.GetValues<SuitSensorMode>();

        while (enumerator.MoveNext(out var containerSlot))
        {
            if (containerSlot.ContainedEntity is not { } item
                || !TryComp<SuitSensorShockableComponent>(item, out var shockable)
                || !TryComp<SuitSensorComponent>(item, out var sensor)
                || sensor.ControlsLocked
                || sensor.User != args.TargetUid)
                continue;

            _suitSensorSystem.SetSensor(new Entity<SuitSensorComponent>(item, sensor), _random.Pick(modes), args.TargetUid);
            _popup.PopupEntity(Loc.GetString("suit-sensor-got-shocked", ("suit", item)),
                args.TargetUid,
                args.TargetUid,
                PopupType.MediumCaution);
        }
    }
}
