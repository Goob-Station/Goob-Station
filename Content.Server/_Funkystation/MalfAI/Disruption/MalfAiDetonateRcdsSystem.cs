// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Content.Server.Administration.Logs;
using Content.Server.Explosion.EntitySystems;
using Content.Shared.Database;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared._Funkystation.MalfAI;
using Content.Shared._Funkystation.MalfAI.Actions;
using Content.Shared.Popups;
using Content.Shared.RCD.Components;
using Content.Shared.Silicons.Borgs.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Server._Funkystation.MalfAI.Disruption;

/// <summary>
/// Detonates all RCDs on the station grid: warns holders, beeps each second,
/// then explodes and deletes them.
/// Engineering borg RCD modules are spared.
/// </summary>
public sealed class MalfAiDetonateRcdsSystem : EntitySystem
{
    [Dependency] private readonly IAdminLogManager _adminLog = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ExplosionSystem _explosions = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedContainerSystem _containers = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    private static readonly TimeSpan RcdDetonationDelay = TimeSpan.FromSeconds(7);
    private static readonly SoundSpecifier RcdBeepSound = new SoundPathSpecifier("/Audio/Effects/beep1.ogg");

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MalfAiMarkerComponent, MalfAiDetonateRcdsActionEvent>(OnDetonateRcds);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<MalfAiArmedRcdComponent, TransformComponent>();
        while (query.MoveNext(out var rcdUid, out var armed, out _))
        {
            if (now >= armed.DetonateTime)
            {
                var coords = _xform.GetMapCoordinates(rcdUid);
                _explosions.QueueExplosion(coords, ExplosionSystem.DefaultExplosionPrototypeId,
                    totalIntensity: 100f, slope: 2f, maxTileIntensity: 50f, cause: null, maxTileBreak: 0);

                QueueDel(rcdUid);
                continue;
            }

            if (now >= armed.NextBeepTime)
            {
                _audio.PlayPvs(RcdBeepSound, rcdUid);
                armed.NextBeepTime += armed.BeepInterval;
            }
        }
    }

    private void OnDetonateRcds(Entity<MalfAiMarkerComponent> ent, ref MalfAiDetonateRcdsActionEvent args)
    {
        if (args.Handled)
            return;

        var xform = Transform(ent.Owner);
        if (xform.GridUid is not { } grid)
            return;

        var armed = 0;
        var now = _timing.CurTime;
        var detonateAt = now + RcdDetonationDelay;

        var query = EntityQueryEnumerator<RCDComponent, TransformComponent>();
        while (query.MoveNext(out var rcdUid, out _, out var rcdXform))
        {
            if (rcdXform.GridUid != grid)
                continue;

            // Spare RCDs that are cyborg modules (engi borg protection).
            if (HasComp<BorgModuleComponent>(rcdUid))
                continue;

            // Warn whoever is holding the RCD.
            if (_containers.TryGetContainingContainer((rcdUid, rcdXform, null), out var container))
            {
                var owner = container.Owner;
                if (TryComp<HandsComponent>(owner, out var hands) && _hands.IsHolding((owner, hands), rcdUid, out _))
                {
                    _popup.PopupEntity(Loc.GetString("detonate_rcd_warning"), owner, owner, PopupType.LargeCaution);
                }
            }

            var rcdArmed = EnsureComp<MalfAiArmedRcdComponent>(rcdUid);
            rcdArmed.DetonateTime = detonateAt;
            rcdArmed.NextBeepTime = now + rcdArmed.BeepInterval;
            armed++;
        }

        if (armed > 0)
        {
            _adminLog.Add(LogType.Action, LogImpact.High,
                $"Malf AI {ToPrettyString(ent.Owner)} armed {armed} RCDs for detonation in {RcdDetonationDelay.TotalSeconds} seconds");
        }

        args.Handled = true;
    }
}
