using System.Threading;
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
using Robust.Shared.Timing;

namespace Content.Server._Lavaland.Weather;

public sealed class LavalandWeatherSystem : EntitySystem
{
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly WeatherSystem _weather = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly TemperatureSystem _temperature = default!;

    private const double LavalandWeatherJobTime = 0.005;
    private readonly JobQueue _lavalandWeatherJobQueue = new(LavalandWeatherJobTime);

    private sealed class LavalandWeatherJob : Job<object>
    {
        private readonly LavalandWeatherSystem _self;
        private readonly Entity<DamageableComponent> _ent;
        private readonly Entity<LavalandStormedMapComponent> _parent;

        public LavalandWeatherJob(LavalandWeatherSystem self,
            Entity<DamageableComponent> ent,
            Entity<LavalandStormedMapComponent> parent,
            double maxTime,
            CancellationToken cancellation = default) : base(maxTime, cancellation)
        {
            _self = self;
            _ent = ent;
            _parent = parent;
        }

        public LavalandWeatherJob(LavalandWeatherSystem self,
            Entity<DamageableComponent> ent,
            Entity<LavalandStormedMapComponent> parent,
            double maxTime,
            IStopwatch stopwatch,
            CancellationToken cancellation = default) : base(maxTime, stopwatch, cancellation)
        {
            _self = self;
            _ent = ent;
            _parent = parent;
        }

        protected override Task<object?> Process()
        {
            _self.ProcessLavalandDamage(_ent, _parent);

            return Task.FromResult<object?>(null);
        }
    }

    private void ProcessLavalandDamage(Entity<DamageableComponent> entity, Entity<LavalandStormedMapComponent> lavaland)
    {
        // Do the damage to all poor people on lava that are not on outpost/big ruins
        if (HasComp<LavalandMemberComponent>(Transform(entity).ParentUid))
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
            _popup.PopupEntity(popup, human, human, PopupType.Large);
        }
    }
}
