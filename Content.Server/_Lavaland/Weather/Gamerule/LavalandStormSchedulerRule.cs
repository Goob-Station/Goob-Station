using Content.Server._Lavaland.Procedural.Prototypes;
using Content.Server._Lavaland.Procedural.Systems;
using Content.Server.Chat.Managers;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Shared.CCVar;
using Content.Shared.GameTicking.Components;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Lavaland.Weather.Gamerule;

public sealed partial class LavalandStormSchedulerRule : GameRuleSystem<LavalandStormSchedulerRuleComponent>
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly LavalandGenerationSystem _lavaland = default!;
    [Dependency] private readonly LavalandWeatherSystem _lavalandWeather = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<LavalandStormSchedulerRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out var uid, out var lavaland, out var gamerule))
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
        if (!_lavaland.GetLavalands(out var maps))
            return;

        var map = _random.Pick(maps);
        if (map.PrototypeId == null)
            return;

        var proto = _proto.Index<LavalandMapPrototype>(map.PrototypeId);
        if (proto.AvailableWeather == null)
            return;

        var weather = _random.Pick(proto.AvailableWeather);

        _lavalandWeather.StartWeather(map, weather);
        _chatManager.SendAdminAlert($"Starting Lavaland Storm for {ToPrettyString(map.Uid)}");
    }

    private void ResetTimer(LavalandStormSchedulerRuleComponent component)
    {
        component.EventClock = RobustRandom.NextFloat(component.Delays.Min, component.Delays.Max);
    }

    protected override void Started(EntityUid uid, LavalandStormSchedulerRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        ResetTimer(component);
    }
    protected override void Ended(EntityUid uid, LavalandStormSchedulerRuleComponent component, GameRuleComponent gameRule, GameRuleEndedEvent args)
    {
        base.Ended(uid, component, gameRule, args);

        ResetTimer(component);
    }
}
