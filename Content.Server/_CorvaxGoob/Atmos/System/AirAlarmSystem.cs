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
                    var indoor = GetEntityQuery<DoorComponent>();
                    var infirelock = GetEntityQuery<FirelockComponent>();
                    foreach (var i in deviceList.Devices)
                    {
                        if (!indoor.TryGetComponent(i, out var nouse) && !infirelock.TryGetComponent(i, out var nouse2))
                            continue;
                        var door = indoor.GetComponent(i); var firelock = infirelock.GetComponent(i);
                        if (door.State == DoorState.Open && this.IsPowered(i, EntityManager))
                            _firelock.EmergencyPressureStop(i);
                    }
                }
            }
        }
    }
}
