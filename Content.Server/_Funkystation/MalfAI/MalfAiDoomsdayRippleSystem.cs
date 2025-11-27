// SPDX-FileCopyrightText: 2025 Dreykor <Dreykor12@gmail.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 funkystationbot <funky@funkystation.org>
//
// SPDX-License-Identifier: MIT

using Content.Server.Body.Components;
using Content.Server.Chat.Systems;
using Content.Server.RoundEnd;
using Content.Shared.Damage;
using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using System.Numerics;
using Content.Shared._Funkystation.MalfAI.Events;

namespace Content.Server._Funkystation.MalfAI;

/// <summary>
/// Handles the completion behavior of the Malfunction AI Doomsday protocol:
/// - Dispatches a global station announcement
/// - Immediately ends the round (respecting round end cvar)
/// - Creates a server-side expanding ripple that kills organic entities (RespiratorComponent)
///   within radius across all grids on the same map, ignoring obstructions
/// - Broadcasts a synchronized client VFX ripple event for visual effects
/// </summary>
public sealed class MalfAiDoomsdayRippleSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly RoundEndSystem _roundEnd = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;

    // Ripple configuration: 300 tiles over 30 seconds.
    private const float MaxRadius = 300f;
    private static readonly TimeSpan Duration = TimeSpan.FromSeconds(30);
    private static readonly float SpeedTilesPerSecond = MaxRadius / (float) Duration.TotalSeconds; // 10 tiles/s

    // Network synchronization buffer: delay server start to align with client VFX
    private const float NetworkSyncDelaySeconds = 0.15f;

    // Active ripple session state (single global ripple at a time).
    private bool _rippleActive;
    private EntityUid _rippleAi;
    private MapId _rippleMap;
    private Vector2 _originWorld;
    private TimeSpan _startTime;
    private readonly HashSet<EntityUid> _affected = new();

    // Prebuilt overwhelming damage to ensure death regardless of resistances.
    private DamageSpecifier _overkill = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MalfAiDoomsdayCompletedEvent>(OnDoomsdayCompleted);

        // Build an "overkill" damage packet (multiple common types) to guarantee lethal impact.
        // Using multiple types reduces the chance of total immunity on atypical entities.
        var overkillAmount = FixedPoint2.New(1000);
        _overkill = new DamageSpecifier
        {
            DamageDict =
            {
                ["Blunt"] = overkillAmount,
                ["Slash"] = overkillAmount,
                ["Piercing"] = overkillAmount,
                ["Heat"] = overkillAmount,
                ["Cold"] = overkillAmount,
                ["Shock"] = overkillAmount,
                ["Poison"] = overkillAmount,
                ["Asphyxiation"] = overkillAmount
            }
        };
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_rippleActive)
            return;

        var elapsed = (_timing.CurTime - _startTime).TotalSeconds;

        // Account for network sync delay - damage starts after clients have time to receive VFX event
        var damageElapsed = elapsed - NetworkSyncDelaySeconds;
        if (damageElapsed <= 0)
            return; // Wait for sync delay to pass before starting damage

        var currentRadius = Math.Clamp((float) damageElapsed * SpeedTilesPerSecond, 0f, MaxRadius);

        // Process all entities with a RespiratorComponent on the same map that have entered the radius.
        var respQuery = EntityQueryEnumerator<RespiratorComponent>();
        while (respQuery.MoveNext(out var target, out _))
        {
            if (_affected.Contains(target))
                continue;

            var xform = Transform(target);

            // Same map/z-level only.
            if (xform.MapID != _rippleMap)
                continue;

            var pos = _xform.GetWorldPosition(xform);
            var posNum = new Vector2(pos.X, pos.Y);
            var dist = (posNum - _originWorld).Length();
            if (dist <= currentRadius)
            {
                // Apply overwhelming damage ignoring resistances; mark processed.
                _damageable.TryChangeDamage(target, _overkill, ignoreResistances: true, origin: _rippleAi);
                _affected.Add(target);
            }
        }

        // Stop the ripple after reaching max radius or total duration (including sync delay) elapses
        if (currentRadius >= MaxRadius || elapsed >= (Duration.TotalSeconds + NetworkSyncDelaySeconds))
            StopRipple();
    }

    /// <summary>
    /// Entry point: announce completion, end round immediately, and start the lethal ripple.
    /// Idempotent: does nothing if a ripple is already active.
    /// </summary>
    /// <param name="station">The station entity (reserved for future use)</param>
    /// <param name="ai">The AI entity that triggered the doomsday protocol</param>
    public void TriggerRippleAndEndRound(EntityUid station, EntityUid ai)
    {
        if (_rippleActive)
            return;

        // Initialize ripple session from AI position.
        var xform = Transform(ai);

        _rippleActive = true;
        _rippleAi = ai;
        _rippleMap = xform.MapID;
        _originWorld = _xform.GetWorldPosition(xform);
        _startTime = _timing.CurTime;
        _affected.Clear();

        // Dispatch the final doomsday completion announcement.
        _chat.DispatchStationAnnouncement(
            ai,
            Loc.GetString("malfai-doomsday-complete"),
            sender: Loc.GetString("malfai-doomsday-sender"),
            playDefaultSound: true,
            colorOverride: Color.Cyan);

        // Tell clients to render the synced ripple, with start time adjusted for network sync delay
        // This ensures client VFX starts when server damage calculation begins
        var ev = new MalfAiDoomsdayRippleStartedEvent(
            _rippleMap,
            _originWorld,
            _timing.CurTime.TotalSeconds + NetworkSyncDelaySeconds,
            Duration,
            MaxRadius,
            centerFlash: true);
        RaiseNetworkEvent(ev, Filter.Broadcast());

        // Immediately trigger round end, respecting round end cvar
        _roundEnd.EndRound();
    }

    /// <summary>
    /// Stops the active ripple and resets the ripple state.
    /// </summary>
    private void StopRipple()
    {
        _rippleActive = false;
        _affected.Clear();
    }

    private void OnDoomsdayCompleted(MalfAiDoomsdayCompletedEvent ev)
    {
        TriggerRippleAndEndRound(ev.Station, ev.Ai);
    }
}
