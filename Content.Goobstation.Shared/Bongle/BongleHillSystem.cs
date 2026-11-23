using Content.Goobstation.Shared.Bongle;
using Content.Shared.Chat;
using Content.Shared.Destructible;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.StepTrigger.Systems;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Container = Robust.Shared.Containers.Container; // i heart ambiguity errors, ambiguous reference between 'Robust.Shared.Containers.Container' and 'System.ComponentModel.Container'
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Bongle;

public sealed partial class BongleHillSystem : EntitySystem
{
    [Dependency] private BongleSystem _bongle = default!;
    [Dependency] private SharedChatSystem _chat = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private MobStateSystem _mob = default!;
    [Dependency] private EntityWhitelistSystem _whitelist = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private SharedAudioSystem _audio = default!;

    [SubscribeLocalEvent]
    private void OnStartup(Entity<BongleHillComponent> ent, ref ComponentStartup args)
    {
        ent.Comp.Container = _container.EnsureContainer<Container>(ent.Owner, ent.Comp.ContainerId);
    }

    [SubscribeLocalEvent]
    private void OnStepTriggerAttempt(Entity<BongleHillComponent> ent, ref StepTriggerAttemptEvent args)
    {
        args.Continue = true;
    }

    [SubscribeLocalEvent]
    private void OnStepTriggered(Entity<BongleHillComponent> ent, ref StepTriggeredOffEvent args)
    {
        var tripper = args.Tripper;

        if (ent.Comp.IncapacitatedOnly && HasComp<MobStateComponent>(tripper) && !_mob.IsIncapacitated(tripper))
            return;

        if (!_container.Insert(tripper, ent.Comp.Container))
            return;

        ent.Comp.Points += GetPoints(ent.Comp, tripper);
        _audio.PlayPredicted(ent.Comp.SuckedInSound, ent, ent);

        if (_mob.IsDead(tripper) && _whitelist.IsWhitelistPass(ent.Comp.TriumphWhitelist, tripper) && !_whitelist.IsWhitelistPass(ent.Comp.TriumphBlacklist, tripper))
        {
            ent.Comp.BingleCorpses++;

            if (ent.Comp.BingleCorpses >= ent.Comp.CorpsesForTriumph)
            {
                ent.Comp.BingleCorpses -= ent.Comp.CorpsesForTriumph;
                _bongle.BoostBongles(ent.Comp.CapacityIncrease, ent.Comp.SpitIncrease);
            }
        }

        if (ent.Comp.Points < ent.Comp.PointsToSpread)
            return;

        ent.Comp.Points -= ent.Comp.PointsToSpread;
        Spread(ent);
    }

    [SubscribeLocalEvent]
    private void OnDestruction(Entity<BongleHillComponent> ent, ref DestructionEventArgs args)
    {
        _container.EmptyContainer(ent.Comp.Container);
    }

    [SubscribeLocalEvent]
    private void Spread(Entity<BongleHillComponent> ent)
    {
        var coords = Transform(ent.Owner).Coordinates.Offset(_random.NextVector2(ent.Comp.SpreadMinRange, ent.Comp.SpreadMaxRange));
        PredictedSpawnAtPosition(ent.Comp.HillProto, coords);

        var hillCount = 0;
        var query = EntityQueryEnumerator<BongleHillComponent>();
        while (query.MoveNext(out _, out _))
        {
            hillCount++;
        }

        _bongle.UpgradeBongles();

        if (hillCount == ent.Comp.AscensionHillCount)
        {
            _bongle.AscendBongles();
            _chat.DispatchGlobalAnnouncement(Loc.GetString("heretic-ascension-bongle"), Name(ent.Owner), true, ent.Comp.AscensionSound, ent.Comp.AnnouncementColor);
        }

        PredictedSpawnAtPosition(ent.Comp.BongleProto, coords);
    }

    private int GetPoints(BongleHillComponent comp, EntityUid target)
    {
        foreach (var value in comp.PointValues)
        {
            if (_whitelist.IsWhitelistPass(value.Whitelist, target))
                return value.Points;
        }

        return comp.DefaultPoints;
    }
}
