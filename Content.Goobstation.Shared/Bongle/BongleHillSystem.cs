using Content.Goobstation.Shared.Bongle;
using Content.Shared.Chat;
using Content.Shared.Destructible;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.StepTrigger.Systems;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Container = Robust.Shared.Containers.Container; // i heart ambiguity errors
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

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BongleHillComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<BongleHillComponent, StepTriggeredOffEvent>(OnStepTriggered);
        SubscribeLocalEvent<BongleHillComponent, StepTriggerAttemptEvent>(OnStepTriggerAttempt);
        SubscribeLocalEvent<BongleHillComponent, DestructionEventArgs>(OnDestruction);
    }

    private void OnStartup(Entity<BongleHillComponent> ent, ref ComponentStartup args)
    {
        var (uid, comp) = ent;

        comp.Container = _container.EnsureContainer<Container>(uid, comp.ContainerId);
    }

    private void OnStepTriggerAttempt(Entity<BongleHillComponent> ent, ref StepTriggerAttemptEvent args)
    {
        args.Continue = true;
    }

    private void OnStepTriggered(Entity<BongleHillComponent> ent, ref StepTriggeredOffEvent args)
    {
        var (_, comp) = ent;
        var tripper = args.Tripper;

        if (comp.IncapacitatedOnly && HasComp<MobStateComponent>(tripper) && !_mob.IsIncapacitated(tripper))
            return;

        if (!_container.Insert(tripper, comp.Container))
            return;

        comp.Points += GetPoints(comp, tripper);
        _audio.PlayPredicted(comp.SuckedInSound, ent, ent);

        if (_mob.IsDead(tripper) && _whitelist.IsWhitelistPass(comp.TriumphWhitelist, tripper) && !_whitelist.IsWhitelistPass(comp.TriumphBlacklist, tripper))
        {
            comp.Corpses++;

            if (comp.Corpses >= comp.CorpsesForTriumph)
            {
                comp.Corpses -= comp.CorpsesForTriumph;
                _bongle.BoostBongles(comp.CapacityIncrease, comp.SpitIncrease);
            }
        }

        if (comp.Points < comp.PointsToSpread)
            return;

        comp.Points -= comp.PointsToSpread;
        Spread(ent);
    }

    private void OnDestruction(Entity<BongleHillComponent> ent, ref DestructionEventArgs args)
    {
        var (_, comp) = ent;

        _container.EmptyContainer(comp.Container);
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

    private void Spread(Entity<BongleHillComponent> ent)
    {
        var (uid, comp) = ent;

        var coords = Transform(uid).Coordinates.Offset(_random.NextVector2(comp.SpreadMinRange, comp.SpreadMaxRange));
        PredictedSpawnAtPosition(comp.HillProto, coords);

        var hillCount = 0;
        var query = EntityQueryEnumerator<BongleHillComponent>();
        while (query.MoveNext(out _, out _))
        {
            hillCount++;
        }

        _bongle.UpgradeBongles();

        if (hillCount == comp.AscensionHillCount)
        {
            _bongle.AscendBongles();
            _chat.DispatchGlobalAnnouncement(Loc.GetString("heretic-ascension-bongle"), Name(uid), true, comp.AscensionSound, comp.AnnouncementColor);
        }

        PredictedSpawnAtPosition(comp.BongleProto, coords);
    }
}
