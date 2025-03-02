using Content.Server.Power.Components;
using Content.Server.Chat.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;
using Content.Server.Station.Systems;
using Content.Server.Power.EntitySystems;
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

    private const float MiningInterval = 10.0f;
    private const float PowerDraw = 10000f;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TelecrystalMinerComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, TelecrystalMinerComponent component, ComponentStartup args)
    {
        component.StartTime = _gameTiming.CurTime;
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<TelecrystalMinerComponent, BatteryComponent, PowerConsumerComponent, TransformComponent>();
        var currentTime = _gameTiming.CurTime;

        while (query.MoveNext(out var entity, out var miner, out var battery, out var powerConsumer, out var transform))
        {
            // Shitcode taken from PowerSinkSystem for it to work (im too lazy to implement it normaly)
            if (!transform.Anchored)
                continue;

            _batterySystem.SetCharge(entity, battery.CurrentCharge + powerConsumer.DrawRate / 1000, battery);

            var elapsed = (currentTime - miner.LastUpdate).TotalSeconds;
            if (elapsed < MiningInterval)
                continue;

            miner.LastUpdate = currentTime;
            miner.AccumulatedTC += 1;

            if (!_containerSystem.TryGetContainer(entity, "tc_slot", out var container))
                continue;

            var found = false;
            foreach (var ent in container.ContainedEntities)
            {
                if (TryComp(ent, out StackComponent? stack))
                {
                    _stackSystem.SetCount(ent, stack.Count + 1);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                var newTC = EntityManager.SpawnEntity("Telecrystal", Transform(entity).Coordinates);
                if (TryComp(newTC, out StackComponent? newStack))
                {
                    _stackSystem.SetCount(newTC, 1);
                    _containerSystem.Insert(newTC, container);
                }
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
