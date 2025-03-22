using Content.Server.Power.Components;
using Content.Server.Chat.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;
using Content.Server.Station.Systems;
using Content.Server.Power.EntitySystems;
using Content.Shared.Examine;
using Robust.Server.Containers;
using Robust.Shared.Containers;
using Content.Shared.Stacks;
using Content.Server.Stack;

namespace Content.Server._Goobstation.Miner;

public sealed class TelecrystalMinerSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly StackSystem _stackSystem = default!;
    [Dependency] private readonly BatterySystem _batterySystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TelecrystalMinerComponent, MapInitEvent>(OnStartup);
    }

    private void OnStartup(EntityUid uid, TelecrystalMinerComponent component, MapInitEvent args)
    {
        var originStation = _station.GetOwningStation(uid);

        if (originStation != null)
        {
            component.OriginStation = originStation;
        } // shitcode from nukesystem
        else
        {
            var transform = Transform(uid);
            component.OriginMapGrid = (transform.MapID, transform.GridUid);
        }

        component.StartTime = _gameTiming.CurTime;
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<TelecrystalMinerComponent, BatteryComponent, PowerConsumerComponent>();
        var currentTime = _gameTiming.CurTime;

        while (query.MoveNext(out var entity, out var miner, out var battery, out var powerConsumer))
        {
            // checking station
            var currentStation = _station.GetOwningStation(entity);
            if (currentStation == null || miner.OriginStation != null && currentStation != miner.OriginStation)
            {
                // if not on station, then L bozo
                continue;
            }

            powerConsumer.NetworkLoad.DesiredPower = miner.PowerDraw;
            if (powerConsumer.NetworkLoad.ReceivingPower < miner.PowerDraw)
                continue;

            _batterySystem.SetCharge(entity, miner.PowerDraw, battery);

            var elapsed = (currentTime - miner.LastUpdate).TotalSeconds;
            if (elapsed < miner.MiningInterval)
                continue;

            miner.LastUpdate = currentTime;
            miner.AccumulatedTC += 1;

            if (!_containerSystem.TryGetContainer(entity, "tc_slot", out var container))
                continue;

            if (container.ContainedEntities.Count == 0)
            {
                var newTC = _entityManager.SpawnEntity("Telecrystal", Transform(entity).Coordinates);
                if (TryComp(newTC, out StackComponent? newStack))
                {
                    _stackSystem.SetCount(newTC, 1);
                    _containerSystem.Insert(newTC, container);
                }
            }
            else if (TryComp(container.ContainedEntities[0], out StackComponent? stack))
            {
                _stackSystem.SetCount(container.ContainedEntities[0], stack.Count + 1);
            }

            if (!miner.Notified && miner.StartTime != null && (currentTime - miner.StartTime.Value).TotalMinutes >= 10)
            {
                miner.Notified = true;
                var station = _station.GetOwningStation(entity);
                if (station != null)
                {
                    _chat.DispatchStationAnnouncement(
                        station.Value,
                        Loc.GetString("powersink-imminent-explosion-announcement"),
                        playDefaultSound: true,
                        colorOverride: Color.Yellow
                    );
                }
            }

            miner.AccumulatedTC = 0;
            _audio.PlayPvs(miner.MiningSound, entity);
        }
    }
}
