using Content.Goobstation.Server.Antag;
using Content.Goobstation.Shared.Gangwars.Components;
using Content.Goobstation.Shared.Gangwars.Events;
using Content.Server._DV.CartridgeLoader.Cartridges;
using Content.Server.CartridgeLoader;
using Content.Server.GameTicking.Rules;
using Content.Shared.Access.Components;
using Content.Shared.PDA;
using Content.Server.Pinpointer;
using Content.Shared.Radio.Components;
using Content.Server.Radio.EntitySystems;
using Content.Shared._DV.NanoChat;
using Content.Shared.SSDIndicator;
using Content.Shared.Station;
using Robust.Server.Audio;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Gangwars.Systems;

public sealed class ServerGangwarRuleSystem : GameRuleSystem<GangwarRuleComponent>
{
    [Dependency] private readonly NavMapSystem _navMap = default!;
    [Dependency] private readonly RadioSystem _radio = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly AntagBetterRandomSpawnSystem _betterRandomSpawn = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly NanoChatCartridgeSystem _nanoChatCartridge = default!;
    [Dependency] private readonly CartridgeLoaderSystem _cartridgeLoader = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedStationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GangTipOffEvent>(OnTipOff);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = QueryActiveRules();
        while (query.MoveNext(out _, out _, out var rule, out _))
        {
            UpdateCrateDrop(rule);
            UpdateDuffelbagDrop(rule);
            UpdatePendingTipOff(rule);
        }
    }

    private void UpdateCrateDrop(GangwarRuleComponent ruleComp)
    {
        UpdateDrop(ruleComp,
            ref ruleComp.NextGangCrateDrop,
            ruleComp.FirstGangCrateDropDelay,
            ruleComp.GangCrateDropInterval,
            ruleComp.GangCratePrototype,
            "gangwar-crate-drop-announcement");
    }

    private void UpdateDuffelbagDrop(GangwarRuleComponent ruleComp)
    {
        UpdateDrop(ruleComp,
            ref ruleComp.NextGangDuffelbagDrop,
            ruleComp.GangDuffelbagDropInterval,
            ruleComp.GangDuffelbagDropInterval,
            ruleComp.GangDuffelbagPrototype,
            "gangwar-duffelbag-drop-announcement");
    }

    private void UpdateDrop(GangwarRuleComponent ruleComp, ref TimeSpan nextDrop, TimeSpan initialDelay, TimeSpan interval, string prototype, string announcementLocKey)
    {
        if (nextDrop == TimeSpan.Zero)
            nextDrop = Timing.CurTime + initialDelay;

        if (Timing.CurTime < nextDrop)
            return;

        nextDrop = Timing.CurTime + interval;
        var dropped = SpawnDrop(ruleComp, prototype);
        if (dropped != null)
            AnnounceGangDrop(ruleComp, GetLocationName(dropped.Value), announcementLocKey);
    }

    private EntityUid? SpawnDrop(GangwarRuleComponent ruleComp, string prototype)
    {
        var attempts = 2000;
        while (attempts-- >= 0)
        {
            if (!_betterRandomSpawn.TryFindSafeRandomLocation(out var coords)
                || _lookup.GetEntitiesInRange<GangLockerComponent>(coords, ruleComp.LockerExclusionRange).Count > 0)
                continue;

            return Spawn(prototype, _transform.ToMapCoordinates(coords));
        }

        return null;
    }

    private string GetLocationName(EntityUid dropped)
    {
        return _navMap.TryGetNearestBeacon((dropped, null), out var beacon, out _) && beacon.Value.Comp.Text != null
            ? beacon.Value.Comp.Text
            : Loc.GetString("gangwar-duffelbag-drop-unknown-location");
    }

    private void AnnounceGangDrop(GangwarRuleComponent ruleComp, string locationName, string announcementLocKey)
    {
        // Empty entity used specifically for the message then deleted
        // Better solutions welcome
        // Who is this boss guy anyways
        var boss = Spawn(null, MapCoordinates.Nullspace);
        _metaData.SetEntityName(boss, ruleComp.AnnouncerName);

        var message = Loc.GetString(announcementLocKey, ("location", locationName));
        _radio.SendRadioMessage(boss, message, ruleComp.GangRadioChannel, boss);
        QueueDel(boss);

        var radioQuery = AllEntityQuery<ActiveRadioComponent>();
        while (radioQuery.MoveNext(out var headset, out var radio))
        {
            if (!radio.Channels.Contains(ruleComp.GangRadioChannel))
                continue;

            var wearer = Transform(headset).ParentUid;
            if (TryComp<ActorComponent>(wearer, out var actor))
                _audio.PlayGlobal(ruleComp.EventAnnouncementSound, actor.PlayerSession);
        }
    }

    /// <summary>
    /// This is used for the buyable duffelbag in the shop
    /// Instead of making the announcement right away it
    /// sends a message to a random crewmember and gives a headstart
    /// </summary>
    private void OnTipOff(GangTipOffEvent ev)
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out _, out _, out var rule, out _))
        {
            rule.NextGangDuffelbagDrop = Timing.CurTime + rule.TipOffDropDelay;

            var dropped = SpawnDrop(rule, rule.GangDuffelbagPrototype);
            if (dropped == null)
                return;

            var locationName = GetLocationName(dropped.Value);
            var dropStation = _station.GetOwningStation(dropped.Value);
            TipOffCivilian(locationName, (int) rule.TipOffHeadstartDelay.TotalSeconds, dropStation);
            rule.TipOffPendingLocation = locationName;
            rule.TipOffAnnounceAt = Timing.CurTime + rule.TipOffHeadstartDelay;
            return;
        }
    }

    private void UpdatePendingTipOff(GangwarRuleComponent rule)
    {
        if (rule.TipOffPendingLocation == null
            || Timing.CurTime < rule.TipOffAnnounceAt)
            return;

        var location = rule.TipOffPendingLocation;
        rule.TipOffPendingLocation = null;
        rule.TipOffAnnounceAt = TimeSpan.Zero;
        AnnounceGangDrop(rule, location, "gangwar-tipoff-drop-announcement");
    }

    private void TipOffCivilian(string locationName, int headstartSeconds, EntityUid? dropStation)
    {
        var candidates = new List<Entity<NanoChatCardComponent>>();
        var cardQuery = AllEntityQuery<NanoChatCardComponent, IdCardComponent>();
        while (cardQuery.MoveNext(out var cardUid, out var card, out _))
        {
            if (!card.ListNumber
                || card.Number == null
                || card.PdaUid == null
                || !TryComp<PdaComponent>(card.PdaUid.Value, out var pda)
                || !PdaHasNanoChat(card.PdaUid.Value))
                continue;

            var owner = pda.PdaOwner;
            if (owner == null
                || HasComp<GangMemberComponent>(owner.Value)
                || !HasComp<ActorComponent>(owner.Value)
                || TryComp<SSDIndicatorComponent>(owner.Value, out var ssd) && ssd.IsSSD
                || _station.GetOwningStation(owner.Value) != dropStation)
                continue;

            candidates.Add((cardUid, card));
        }

        if (candidates.Count == 0)
            return;

        var recipient = _random.Pick(candidates);

        _nanoChatCartridge.DeliverAnonymousMessage(
            recipient,
            0000,
            Loc.GetString("gangwar-tipoff-nanochat-sender"),
            Loc.GetString("gangwar-tipoff-nanochat-message", ("location", locationName), ("seconds", headstartSeconds)));
    }

    private bool PdaHasNanoChat(EntityUid pdaUid) =>
        _cartridgeLoader.HasProgram<NanoChatCartridgeComponent>(pdaUid);
}

