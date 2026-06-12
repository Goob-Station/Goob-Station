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
using Robust.Server.Player;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Server._Funkystation.MalfAI;

/// <summary>
/// Handles the final doomsday completion - sends the expanding lethal ripple across the map,
/// deals damage to all entities, and ends the round.
/// </summary>
public sealed class MalfAiDoomsdayRippleSystem : EntitySystem
{
    [Dependency] private readonly IAdminLogManager _adminLog = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedTransformSystem _xforms = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;

    private const float MaxRadiusTiles = 300f;
    private const float RippleDurationSeconds = 30f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MalfAiDoomsdayCompletedEvent>(OnDoomsdayCompleted);
    }

    private void OnDoomsdayCompleted(MalfAiDoomsdayCompletedEvent ev)
    {
        var ai = ev.Ai;
        if (Deleted(ai))
            return;

        var aiPos = _xforms.GetMapCoordinates(ai);

        // Send visual event to all clients
        var rippleEvent = new MalfAiDoomsdayRippleStartedEvent(
            aiPos.MapId,
            aiPos.Position,
            _timing.CurTime.TotalSeconds,
            RippleDurationSeconds,
            MaxRadiusTiles,
            true);

        RaiseNetworkEvent(rippleEvent);

        _adminLog.Add(LogType.Action, LogImpact.Extreme,
            $"Malf AI Doomsday ripple starting at {aiPos.Position} on map {aiPos.MapId}");

        // Schedule the lethal ripple damage over 30 seconds
        Timer.Spawn(TimeSpan.FromSeconds(RippleDurationSeconds), () =>
        {
            // Deal lethal damage to all damageable entities on the map
            var query = EntityQueryEnumerator<DamageableComponent, TransformComponent>();
            while (query.MoveNext(out var target, out _, out var xform))
            {
                if (xform.MapID != aiPos.MapId)
                    continue;

                _damage.TryChangeDamage(target, new DamageSpecifier
                {
                    DamageDict = new Dictionary<string, FixedPoint2> { { "Radiation", 200f } }
                });
            }

            // End the round
            Timer.Spawn(TimeSpan.FromSeconds(5), () =>
            {
                _gameTicker.EndRound(Loc.GetString("malfai-doomsday-round-end-reason"));
            });
        });
    }
}
