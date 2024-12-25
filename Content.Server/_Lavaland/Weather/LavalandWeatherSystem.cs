using Content.Server._Lavaland.Procedural.Systems;
using Content.Server.GameTicking;
using Content.Server.Weather;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.CCVar;
using Content.Shared.Damage;
using Content.Shared.Humanoid;
using Content.Shared.Popups;
using Robust.Shared.Configuration;
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
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundStartAttemptEvent>(StartStorm);
    }

    private void StartStorm(RoundStartAttemptEvent ev)
    {
        //if (_cfg.GetCVar(CCVars.LavalandEnabled))
            //_gameTicker.StartGameRule("LavalandStormScheduler");
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var maps = EntityQueryEnumerator<LavalandStormedMapComponent>();
        while(maps.MoveNext(out _, out var comp))
        {
            comp.Accumulator += frameTime;

            // End the weather when it's time
            if (comp.Accumulator >= comp.Duration)
            {
                EndWeather(comp.Lavaland, "lavaland-weather-end");
            }

            // Do the damage to all poor people on lava
            if (!(comp.Accumulator >= comp.DamageAccumulator))
                continue;

            var humans = EntityQueryEnumerator<HumanoidAppearanceComponent, DamageableComponent>();
            while (humans.MoveNext(out var human, out _, out var damageable))
            {
                if (Transform(human).ParentUid != comp.Lavaland.Uid)
                    continue;

                _damage.TryChangeDamage(human, comp.Damage, damageable: damageable, interruptsDoAfters: false, partMultiplier: 0.25f, targetPart: TargetBodyPart.All);
                comp.DamageAccumulator = comp.NextDamage;
            }
        }
    }

    public void StartWeather(LavalandMap map, ProtoId<LavalandWeatherPrototype> weather)
    {
        var proto = _proto.Index(weather);

        _weather.SetWeather(map.MapId, _proto.Index(proto.WeatherType), null);

        var comp = EnsureComp<LavalandStormedMapComponent>(map.Uid);
        comp.CurrentWeather = proto.ID;
        comp.Duration = proto.Duration + _random.NextFloat(-proto.Variety, proto.Variety);
        comp.Lavaland = map;
        comp.Damage = proto.Damage;

        var humans = EntityQueryEnumerator<HumanoidAppearanceComponent, DamageableComponent>();
        while (humans.MoveNext(out var human, out _, out _))
        {
            _popup.PopupEntity(proto.PopupMessage, human, human, PopupType.LargeCaution);
        }
    }

    public void EndWeather(LavalandMap map, LocId endMessage)
    {
        _weather.SetWeather(map.MapId, null, null);
        RemComp<LavalandStormedMapComponent>(map.Uid);

        var humans = EntityQueryEnumerator<HumanoidAppearanceComponent, DamageableComponent>();
        while (humans.MoveNext(out var human, out _, out _))
        {
            _popup.PopupEntity(endMessage, human, human, PopupType.SmallCaution);
        }
    }
}
