using System.Threading;
using System.Threading.Tasks;
using Content.Server._Lavaland.Procedural.Components;
using Content.Server.Temperature.Systems;
using Content.Server.Weather;
using Content.Shared._Lavaland.Weather;
using Content.Shared.Damage;
using Content.Shared.Humanoid;
using Content.Shared.Popups;
using Robust.Shared.CPUJob.JobQueues;
using Robust.Shared.CPUJob.JobQueues.Queues;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Lavaland.Weather;

public sealed class LavalandWeatherSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly WeatherSystem _weather = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly TemperatureSystem _temperature = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;

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
        if (xform.GridUid != lavaland.Owner)
            return;

        var proto = _proto.Index(lavaland.Comp.CurrentWeather);
        _temperature.ChangeHeat(entity, proto.TemperatureChange, ignoreHeatResistance: true);
        _damage.TryChangeDamage(entity, proto.Damage);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        _lavalandWeatherJobQueue.Process();

        var maps = EntityQueryEnumerator<LavalandMapComponent, LavalandStormedMapComponent>();
        while(maps.MoveNext(out var map, out var lavaland, out var comp))
        {
            UpdateWeatherDuration(frameTime, (map, lavaland), comp);
            UpdateWeatherDamage(frameTime, (map, comp));
        }
    }
    private void UpdateWeatherDuration(float frameTime, Entity<LavalandMapComponent> map, LavalandStormedMapComponent comp)
    {
        comp.Accumulator += frameTime;
        if (comp.Accumulator >= comp.Duration)
        {
            EndWeather(map);
        }
    }
    private void UpdateWeatherDamage(float frameTime, Entity<LavalandStormedMapComponent> stormedMap)
    {
        var comp = stormedMap.Comp;
        comp.DamageAccumulator += frameTime;

        if (comp.DamageAccumulator <= comp.NextDamage)
            return;

        var humans = EntityQueryEnumerator<HumanoidAppearanceComponent, DamageableComponent>();

        while (humans.MoveNext(out var human, out _, out var damageable))
        {
            _lavalandWeatherJobQueue.EnqueueJob(new LavalandWeatherJob(this, (human, damageable), stormedMap, LavalandWeatherJobTime));
        }

        comp.DamageAccumulator = 0;
    }

    public void StartWeather(Entity<LavalandMapComponent> map, ProtoId<LavalandWeatherPrototype> weather)
    {
        if (HasComp<LavalandStormedMapComponent>(map))
            return;

        var proto = _proto.Index(weather);

        _weather.SetWeather(Transform(map).MapID, _proto.Index(proto.WeatherType), null);

        Log.Debug($"Starting dealing weather damage on lavaland map {ToPrettyString(map)}");
        var comp = EnsureComp<LavalandStormedMapComponent>(map);
        comp.CurrentWeather = proto.ID;
        comp.Duration = proto.Duration + _random.NextFloat(-proto.Variety, proto.Variety);

        var humans = EntityQueryEnumerator<HumanoidAppearanceComponent, DamageableComponent>();
        while (humans.MoveNext(out var human, out _, out _))
        {
            var xform = Transform(human);
            if (xform.MapUid != map.Owner)
                continue;

            _popup.PopupEntity(Loc.GetString(proto.PopupStartMessage), human, human, PopupType.LargeCaution);
        }
    }

    public void EndWeather(Entity<LavalandMapComponent> map)
    {
        _weather.SetWeather(Transform(map).MapID, null, null);
        if (!TryComp<LavalandStormedMapComponent>(map, out var comp))
            return;

        var popup = Loc.GetString(_proto.Index(comp.CurrentWeather).PopupEndMessage);
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
