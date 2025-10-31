using Content.Server.Atmos.Monitor.Components;
using Content.Server.Doors.Systems;
using Content.Server.Power.EntitySystems;
using Content.Shared.Atmos.Monitor;
using Content.Shared.DeviceNetwork.Components;
using Content.Shared.Doors.Components;

namespace Content.Server.Atmos.Monitor.Systems
{
    public partial class AirAlarmSystem : EntitySystem
    {
        [Dependency] private readonly FirelockSystem _firelock = default!;
        public void AlarmforOpenFirelocks()
        {
            var query = EntityQueryEnumerator<AtmosAlarmableComponent, DeviceListComponent>();
            while (query.MoveNext(out var uid, out var atmosAlarmable, out var deviceList))
            {
                if (atmosAlarmable.LastAlarmState == AtmosAlarmType.Danger && this.IsPowered(uid, EntityManager))
                {
                    foreach (EntityUid сonnectedEnt in deviceList.Devices)
                    {
                        if (TryComp<FirelockComponent>(сonnectedEnt, out var flc) &
                            TryComp<DoorComponent>(сonnectedEnt, out var door) &
                            this.IsPowered(сonnectedEnt, EntityManager))
                            _firelock.EmergencyPressureStop(сonnectedEnt, flc, door);
                    }
                }
            }
        }
    }
}
