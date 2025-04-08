using System.Linq;
using Content.Server._Lavaland.Procedural.Systems;
using Content.Server.Chat.Managers;
using Content.Server.GameTicking.Rules;
using Content.Shared._Lavaland.Procedural.Prototypes;
using Content.Shared.GameTicking.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Lavaland.Weather.Gamerule;

public sealed class LavalandStormSchedulerRule : GameRuleSystem<LavalandStormSchedulerRuleComponent>
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly LavalandPlanetSystem _lavaland = default!;
    [Dependency] private readonly LavalandWeatherSystem _lavalandWeather = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<LavalandStormSchedulerRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out _, out var lavaland, out _))
        {
            lavaland.EventClock -= frameTime;
            if (lavaland.EventClock <= 0)
            {
                StartRandomStorm();
                ResetTimer(lavaland);
            }
        }
    }

    private void StartRandomStorm()
    {
        var maps = _lavaland.GetLavalands();
        if (maps.Count == 0)
            return;

        // Filter out already stormed maps.
        var newMaps = maps.Where(lavaland => !HasComp<LavalandStormedMapComponent>(lavaland)).ToList();
        maps = newMaps;

        var map = _random.Pick(maps);
        if (map.Comp.PrototypeId == null)
            return;

        var proto = _proto.Index<LavalandMapPrototype>(map.Comp.PrototypeId);
        if (proto.AvailableWeather == null)
            return;

        var weather = _random.Pick(proto.AvailableWeather);

        _lavalandWeather.StartWeather(map, weather);
        _chatManager.SendAdminAlert($"Starting Lavaland Storm for {ToPrettyString(map)}");
    }

    private void ResetTimer(LavalandStormSchedulerRuleComponent component)
    {
        component.EventClock = RobustRandom.NextFloat(component.Delays.Min, component.Delays.Max);
    }

    protected override void Started(EntityUid uid, LavalandStormSchedulerRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        ResetTimer(component);

        if (_lavaland.LavalandEnabled)
        {
            _lavaland.EnsurePreloaderMap();
            _lavaland.SetupLavalands();
        }
    }
    protected override void Ended(EntityUid uid, LavalandStormSchedulerRuleComponent component, GameRuleComponent gameRule, GameRuleEndedEvent args)
    {
        base.Ended(uid, component, gameRule, args);

        ResetTimer(component);
    }
}
