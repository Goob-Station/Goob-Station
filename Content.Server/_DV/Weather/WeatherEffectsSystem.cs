using Content.Shared.Damage;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Weather;
using Content.Shared.Whitelist;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._DV.Weather;

/// <summary>
/// Handles weather damage for exposed entities.
/// </summary>
public sealed partial class WeatherEffectsSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedWeatherSystem _weather = default!;

    private EntityQuery<MapGridComponent> _gridQuery;

    public override void Initialize()
    {
        base.Initialize();

        _gridQuery = GetEntityQuery<MapGridComponent>();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<WeatherComponent>();
        while (query.MoveNext(out var map, out var weather))
        {
            if (now < weather.NextUpdate)
                continue;

            weather.NextUpdate = now + weather.UpdateDelay;

            foreach (var (id, data) in weather.Weather)
            {
                // start and end do no damage
                if (data.State != WeatherState.Running)
                    continue;

                UpdateDamage(map, id);
            }
        }
    }

    private void UpdateDamage(EntityUid map, ProtoId<WeatherPrototype> id)
    {
        var weather = _proto.Index(id);
        if (weather.Damage is not {} damage)
            return;

        var query = EntityQueryEnumerator<MobStateComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var mob, out var xform))
        {
            // don't give dead bodies 10000 burn, that's not fun for anyone
            if (xform.MapUid != map || mob.CurrentState == MobState.Dead)
                continue;

            // if not in space, check for being indoors
            if (xform.GridUid is {} gridUid && _gridQuery.TryComp(gridUid, out var grid))
            {
                var tile = _map.GetTileRef((gridUid, grid), xform.Coordinates);
                if (!_weather.CanWeatherAffect(gridUid, grid, tile))
                    continue;
            }

            if (_whitelist.IsBlacklistFailOrNull(weather.DamageBlacklist, uid))
                _damageable.TryChangeDamage(uid, damage, interruptsDoAfters: false);
        }
    }
}
