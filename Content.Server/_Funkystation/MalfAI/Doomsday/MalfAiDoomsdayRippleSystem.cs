// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Content.Goobstation.Maths.FixedPoint;
using Content.Server.Administration.Logs;
using Content.Server.GameTicking;
using Content.Shared.Damage;
using Content.Shared.Database;
using Content.Shared._Funkystation.MalfAI;
using Content.Shared._Funkystation.MalfAI.Doomsday;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Server._Funkystation.MalfAI.Doomsday;

/// <summary>
/// Handles the final doomsday completion - sends the expanding lethal ripple across the map,
/// deals damage to all entities, and ends the round.
/// </summary>
public sealed class MalfAiDoomsdayRippleSystem : EntitySystem
{
    [Dependency] private readonly IAdminLogManager _adminLog = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly SharedTransformSystem _xforms = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;

    private const float MaxRadiusTiles = 300f;
    private const float RippleDurationSeconds = 30f;
    private static readonly TimeSpan RoundEndDelay = TimeSpan.FromSeconds(5);

    private static readonly DamageSpecifier RippleDamage = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2> { { "Radiation", 200f } }
    };

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MalfAiDoomsdayCompletedEvent>(OnDoomsdayCompleted);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<MalfAiDoomsdayRipplePendingComponent>();
        while (query.MoveNext(out var uid, out var ripple))
        {
            if (!ripple.DamageDealt && now >= ripple.DamageTime)
            {
                ripple.DamageDealt = true;
                DealRippleDamage(ripple.TargetMapId);
            }

            if (now >= ripple.RoundEndTime)
            {
                _gameTicker.EndRound(Loc.GetString("malfai-doomsday-round-end-reason"));
                RemComp<MalfAiDoomsdayRipplePendingComponent>(uid);
            }
        }
    }

    private void OnDoomsdayCompleted(ref MalfAiDoomsdayCompletedEvent ev)
    {
        var ai = ev.Ai;
        if (Deleted(ai))
            return;

        var aiPos = _xforms.GetMapCoordinates(ai);
        var now = _timing.CurTime;

        // Send visual event to all clients
        RaiseNetworkEvent(new MalfAiDoomsdayRippleStartedEvent(
            aiPos.MapId,
            aiPos.Position,
            now.TotalSeconds,
            RippleDurationSeconds,
            MaxRadiusTiles,
            true));

        _adminLog.Add(LogType.Action, LogImpact.Extreme,
            $"Malf AI Doomsday ripple starting at {aiPos.Position} on map {aiPos.MapId}");

        var pending = EnsureComp<MalfAiDoomsdayRipplePendingComponent>(ai);
        pending.TargetMapId = aiPos.MapId;
        pending.OriginPos = aiPos.Position;
        pending.DamageTime = now + TimeSpan.FromSeconds(RippleDurationSeconds);
        pending.RoundEndTime = pending.DamageTime + RoundEndDelay;
        pending.DamageDealt = false;
    }

    private void DealRippleDamage(MapId mapId)
    {
        var query = EntityQueryEnumerator<DamageableComponent, TransformComponent>();
        while (query.MoveNext(out var target, out _, out var xform))
        {
            if (xform.MapID != mapId)
                continue;

            _damage.TryChangeDamage(target, RippleDamage);
        }
    }
}
