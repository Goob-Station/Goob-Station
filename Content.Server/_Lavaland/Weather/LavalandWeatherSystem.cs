﻿using System.Threading;
using System.Threading.Tasks;
using Content.Server._Lavaland.Procedural.Components;
using Content.Server.GameTicking;
using Content.Server.Temperature.Systems;
using Content.Server.Weather;
using Content.Shared.CCVar;
using Content.Shared.Damage;
using Content.Shared.Humanoid;
using Content.Shared.Popups;
using Robust.Shared.Configuration;
using Robust.Shared.CPUJob.JobQueues;
using Robust.Shared.CPUJob.JobQueues.Queues;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Lavaland.Weather;

public sealed class LavalandWeatherSystem : EntitySystem
{
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly WeatherSystem _weather = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly TemperatureSystem _temperature = default!;

    private const double LavalandWeatherJobTime = 0.005;
    private readonly JobQueue _lavalandWeatherJobQueue = new(LavalandWeatherJobTime);

    private sealed class LavalandWeatherJob(
        LavalandWeatherSystem self,
        Entity<DamageableComponent> ent,
        Entity<LavalandStormedMapComponent> parent,
        double maxTime,
        CancellationToken cancellation = default)
        : Job<object>(maxTime, cancellation)
    {
        protected override Task<object?> Process()
        {
            self.ProcessLavalandDamage(ent, parent);

            return Task.FromResult<object?>(null);
        }
    }

    private void ProcessLavalandDamage(Entity<DamageableComponent> entity, Entity<LavalandStormedMapComponent> lavaland)
    {
        var xform = Transform(entity);
        // Do the damage to all poor people on lava that are not on outpost/big ruins
        if (xform.MapUid != lavaland.Owner || HasComp<LavalandMemberComponent>(xform.ParentUid))
            return;

        var proto = _proto.Index(lavaland.Comp.CurrentWeather);
        _temperature.ChangeHeat(entity, proto.TemperatureChange);
    }

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundStartAttemptEvent>(StartStorm);
    }

    private void StartStorm(RoundStartAttemptEvent ev)
    {
        if (_cfg.GetCVar(CCVars.LavalandEnabled))
            _gameTicker.StartGameRule("LavalandStormScheduler");
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        _lavalandWeatherJobQueue.Process();

        var maps = EntityQueryEnumerator<LavalandMapComponent, LavalandStormedMapComponent>();
        while(maps.MoveNext(out var map,out var lavaland, out var comp))
        {
            comp.Accumulator += frameTime;

            // End the weather when it's time
            if (comp.Accumulator >= comp.Duration)
            {
                EndWeather((map, lavaland));
            }

            comp.DamageAccumulator += frameTime;

            if (comp.DamageAccumulator <= comp.NextDamage)
                continue;

            var humans = EntityQueryEnumerator<HumanoidAppearanceComponent, DamageableComponent>();
            while (humans.MoveNext(out var human, out _, out var damageable))
            {
                _lavalandWeatherJobQueue.EnqueueJob(new LavalandWeatherJob(this, (human, damageable), (map, comp),  LavalandWeatherJobTime));
            }

            comp.DamageAccumulator = 0;
        }
    }

    public void StartWeather(Entity<LavalandMapComponent> map, ProtoId<LavalandWeatherPrototype> weather)
    {
        if (HasComp<LavalandStormedMapComponent>(map))
            return;

        var proto = _proto.Index(weather);

        _weather.SetWeather(map.Comp.MapId, _proto.Index(proto.WeatherType), null);

        var comp = EnsureComp<LavalandStormedMapComponent>(map);
        comp.CurrentWeather = proto.ID;
        comp.Duration = proto.Duration + _random.NextFloat(-proto.Variety, proto.Variety);

        var humans = EntityQueryEnumerator<HumanoidAppearanceComponent, DamageableComponent>();
        while (humans.MoveNext(out var human, out _, out _))
        {
            var xform = Transform(human);
            if (xform.MapUid != map.Owner)
                continue;

            _popup.PopupEntity(proto.PopupStartMessage, human, human, PopupType.LargeCaution);
        }
    }

    public void EndWeather(Entity<LavalandMapComponent> map)
    {
        _weather.SetWeather(map.Comp.MapId, null, null);
        if (!TryComp<LavalandStormedMapComponent>(map, out var comp))
            return;

        var popup = _proto.Index(comp.CurrentWeather).PopupEndMessage;
        RemComp<LavalandStormedMapComponent>(map);

        var humans = EntityQueryEnumerator<HumanoidAppearanceComponent, DamageableComponent>();
        while (humans.MoveNext(out var human, out _, out _))
        {
            var xform = Transform(human);
            if (xform.MapUid != map.Owner)
                continue;

            _popup.PopupEntity(popup, human, human, PopupType.Large);
        }
    }
}
