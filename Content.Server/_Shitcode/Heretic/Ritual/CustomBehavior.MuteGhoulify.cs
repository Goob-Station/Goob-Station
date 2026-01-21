// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared.Heretic;
using Content.Shared.Heretic.Prototypes;
using Content.Shared.Rejuvenate;
using Content.Shared.Speech.Muting;

namespace Content.Server.Heretic.Ritual;

public sealed partial class RitualMuteGhoulifyBehavior : RitualSacrificeBehavior
{
    public override bool Execute(RitualData args, out string? outstr)
    {
        var limitExceeded = args is { Limit: > 0, Limited: not null } && args.Limited.Count >= args.Limit;
        outstr = null;

        if (!limitExceeded && base.Execute(args, out _))
            return true;

        uids.Clear();

        var lookup = _lookup.GetEntitiesInRange(args.Platform, 1.5f);
        foreach (var look in lookup)
        {
            if (args.Limited?.Contains(look) is true)
                uids.Add(look);
        }

        if (uids.Count > 0)
            return true;

        outstr = Loc.GetString("heretic-ritual-fail-sacrifice");

        return false;
    }

    public override void Finalize(RitualData args)
    {
        foreach (var uid in uids)
        {
            if (args.Limited?.Contains(uid) is true)
            {
                args.EntityManager.EventBus.RaiseLocalEvent(uid, new RejuvenateEvent());
                continue;
            }

            var limitExceeded = args is { Limit: > 0, Limited: not null } && args.Limited.Count >= args.Limit;

            if (limitExceeded || args.EntityManager.HasComponent<GhoulComponent>(uid))
                continue;

            var ghoul = new GhoulComponent
            {
                TotalHealth = 150f,
                GiveBlade = true,
                BoundHeretic = args.Performer,
                DeathBehavior = GhoulDeathBehavior.NoGib,
            };
            args.EntityManager.AddComponent(uid, ghoul, overwrite: true);
            args.EntityManager.EnsureComponent<MutedComponent>(uid);
            args.EntityManager.EnsureComponent<HereticBladeUserBonusDamageComponent>(uid);
            args.EntityManager.EnsureComponent<VoicelessDeadComponent>(uid);

            args.Limited?.Add(uid);
        }
    }

    public override bool LimitExceeded(RitualData args, out bool ritualSuccess)
    {
        ritualSuccess = false;
        if (args.Limited == null)
            return true;

        var xformsys = args.EntityManager.System<SharedTransformSystem>();

        var coords = xformsys.GetMapCoordinates(args.Platform);
        EntityUid? selectedGhoul = null;
        var maxDist = 1.5f;

        foreach (var ghoul in args.Limited)
        {
            if (!args.EntityManager.EntityExists(ghoul))
                continue;

            var ghoulCoords = xformsys.GetMapCoordinates(ghoul);
            if (ghoulCoords.MapId != coords.MapId)
            {
                selectedGhoul = ghoul;
                break;
            }

            var dist = (coords.Position - ghoulCoords.Position).Length();

            if (dist < 1.5f) // Ghoul is already on the rune, attempt to heal it
                return false;

            if (dist < maxDist)
                continue;

            maxDist = dist;
            selectedGhoul = ghoul;
        }

        if (selectedGhoul is not { } selected)
            return true;

        xformsys.SetMapCoordinates(selected, coords);
        ritualSuccess = true;
        return true;
    }
}
