using Content.Server.Chat.Managers;
using Content.Shared.Chat;
using Content.Shared.Weather;
using Robust.Shared.Map.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._DV.Weather;

public sealed class WeatherSchedulerSystem : EntitySystem
{
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedWeatherSystem _weather = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<WeatherSchedulerComponent>();
        while (query.MoveNext(out var map, out var comp))
        {
            if (now < comp.NextUpdate)
                continue;

            if (comp.Stage >= comp.Stages.Count)
                comp.Stage = 0;

            var stage = comp.Stages[comp.Stage++];
            var duration = stage.Duration.Next(_random);
            comp.NextUpdate = now + TimeSpan.FromSeconds(duration);

            var mapId = Comp<MapComponent>(map).MapId;
            if (stage.Weather is {} weather)
            {
                var ending = comp.NextUpdate;
                // crossfade weather so as one ends the next starts
                if (HasWeather(comp, comp.Stage - 1))
                    ending += WeatherComponent.ShutdownTime;
                if (HasWeather(comp, comp.Stage + 1))
                    ending += WeatherComponent.StartupTime;
                _weather.SetWeather(mapId, _proto.Index(weather), ending);
            }

            if (stage.Message is {} message)
            {
                var msg = Loc.GetString(message);
                _chat.ChatMessageToManyFiltered(
                    Filter.BroadcastMap(mapId),
                    ChatChannel.Radio,
                    msg,
                    msg,
                    map,
                    false,
                    true,
                    null);
            }
        }
    }

    private bool HasWeather(WeatherSchedulerComponent comp, int stage)
    {
        if (stage < 0)
            stage = comp.Stages.Count + stage;
        else if (stage >= comp.Stages.Count)
            stage %= comp.Stages.Count;

        return comp.Stages[stage].Weather != null;
    }
}
