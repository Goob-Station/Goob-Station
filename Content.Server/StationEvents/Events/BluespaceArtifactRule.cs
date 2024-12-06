using Content.Server.StationEvents.Components;
﻿using Content.Shared.GameTicking.Components;
using Robust.Shared.Random;
using Content.Server._Goobstation.Announcements.Systems; // Goobstation - Custom Announcers
using Robust.Shared.Player; // Goobstation - Custom Announcers

namespace Content.Server.StationEvents.Events;

public sealed class BluespaceArtifactRule : StationEventSystem<BluespaceArtifactRuleComponent>
{
    [Dependency] private readonly AnnouncerSystem _announcer = default!; // Goobstation - Custom Announcers

    protected override void Added(EntityUid uid, BluespaceArtifactRuleComponent component, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        if (!TryComp<StationEventComponent>(uid, out var stationEvent))
            return;

        _announcer.SendAnnouncement(
            _announcer.GetAnnouncementId(args.RuleId),
            Filter.Broadcast(),
            "bluespace-artifact-event-announcement",
            null,
            Color.FromHex("#18abf5"),
            null, null,
            ("sighting", Loc.GetString(RobustRandom.Pick(component.PossibleSighting)))
        ); // Goobstation - Custom Announcers

        base.Added(uid, component, gameRule, args);
    }

    protected override void Started(EntityUid uid, BluespaceArtifactRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        var amountToSpawn = 1;
        for (var i = 0; i < amountToSpawn; i++)
        {
            if (!TryFindRandomTile(out _, out _, out _, out var coords))
                return;

            Spawn(component.ArtifactSpawnerPrototype, coords);
            Spawn(component.ArtifactFlashPrototype, coords);

            Sawmill.Info($"Spawning random artifact at {coords}");
        }
    }
}
