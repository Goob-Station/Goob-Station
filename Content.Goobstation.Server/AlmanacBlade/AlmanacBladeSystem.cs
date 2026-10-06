using System.Linq;
using Content.Goobstation.Common.AlmanacBlade;
using Content.Goobstation.Shared.AlmanacBlade;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Explosion.EntitySystems;
using Content.Server.GameTicking;
using Content.Shared._White.Blink;
using Content.Shared.Atmos.Components;
using Content.Shared.GameTicking;
using Content.Shared.Hands;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Materials;
using Content.Shared.Storage.Components;
using Content.Shared.Storage.EntitySystems;
using Content.Shared.Tag;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Xenoarchaeology.Artifact;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.AlmanacBlade;

public sealed class AlmanacBladeSystem : SharedAlmanacBladeSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly ExplosionSystem _explosion = default!;
    [Dependency] private readonly GameTicker _ticker = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedEntityStorageSystem _entityStorage = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    private static readonly ProtoId<TagPrototype> OreTag = "Ore";
    private static readonly string[] CargoPresets = { "Extended", "Greenshift" };

    private const string CargoProduct = "FunAlmanacBlade";
    private const string BladePrototype = "WeaponAlmanacBlade";
    private const string MaintsClosetPrefix = "ClosetMaintenance";
    private const int MaxFlipsForBlink = 19;
    private const int HarvestsForMaints = 15;
    private const float MaintsChance = 0.0005f;
    private const int OresForPresents = 666;
    private const int MaxReadiedForCargo = 30;

    private static readonly TimeSpan RefreshInterval = TimeSpan.FromSeconds(15);

    private TimeSpan _nextRefresh;
    private int _flips;
    private int _harvests;
    private int _ores;
    private int _readied;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AlmanacBladeComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<AlmanacBladeComponent, MeleeHitEvent>(OnMeleeHit);
        SubscribeLocalEvent<AlmanacBladeComponent, GotEquippedHandEvent>(OnEquippedHand);

        SubscribeLocalEvent<AlmanacBladeArtifactSpawnerComponent, XenoArtifactNodeActivatedEvent>(OnArtifactActivated);
        SubscribeLocalEvent<AlmanacBladeChestSpawnerComponent, MapInitEvent>(OnChestMapInit);
        SubscribeLocalEvent<AlmanacOreCounterComponent, MaterialEntityInsertedEvent>(OnMaterialInserted);

        SubscribeLocalEvent<PlantHarvestedEvent>(OnPlantHarvested);
        SubscribeLocalEvent<GetHiddenCargoProductsEvent>(OnGetHiddenCargoProducts);
        SubscribeLocalEvent<RulePlayerJobsAssignedEvent>(OnJobsAssigned);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
    }

    public void RecordFlip()
    {
        _flips++;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_timing.CurTime < _nextRefresh)
            return;

        _nextRefresh = _timing.CurTime + RefreshInterval;

        var query = EntityQueryEnumerator<AlmanacBladeComponent, MeleeWeaponComponent>();
        while (query.MoveNext(out var uid, out var blade, out var melee))
        {
            Refresh((uid, blade, melee));
        }
    }

    private void Refresh(Entity<AlmanacBladeComponent, MeleeWeaponComponent> ent)
    {
        if (ent.Comp1.FreezeCalendar)
            return;

        var local = DateTime.Now;
        var date = DateOnly.FromDateTime(local);
        var blade = ent.Comp1;
        var melee = ent.Comp2;

        var rate = AlmanacCalendar.IsEclipse(date)
            ? blade.EclipseAttackRate
            : MathHelper.Lerp(blade.NewMoonAttackRate, blade.FullMoonAttackRate, (float) AlmanacCalendar.MoonIllumination(DateTime.UtcNow));
        var wednesday = date.DayOfWeek == DayOfWeek.Wednesday;

        if (!MathHelper.CloseToPercent(melee.AttackRate, rate) || melee.CanWideSwing != wednesday)
        {
            melee.AttackRate = rate;
            melee.CanWideSwing = wednesday;
            Dirty(ent, melee);
        }

        blade.LeapDay = AlmanacCalendar.IsLeapDay(date);
        blade.Afternoon = local.Hour >= 12;
        blade.Christmas = AlmanacCalendar.IsChristmas(date);
        blade.HardBassDay = AlmanacCalendar.IsSaturdayAfterEaster(date);
        blade.WednesdayBeforeChristmas = AlmanacCalendar.IsWednesdayBeforeChristmas(date);
        blade.MondayBeforeGarfield = AlmanacCalendar.IsMondayBeforeGarfield(date);
        blade.AprilFoolsOver = AlmanacCalendar.IsAprilFoolsOver(local);
        Dirty(ent, blade);

        var canBlink = date.DayOfWeek == DayOfWeek.Tuesday && _flips <= MaxFlipsForBlink;
        if (canBlink)
            EnsureComp<BlinkComponent>(ent);
        else
            RemComp<BlinkComponent>(ent);

        if (blade.AprilFoolsOver && Transform(ent).ParentUid is { Valid: true } holder && _hands.IsHolding(holder, ent.Owner))
            Explode((ent.Owner, blade));
    }

    private void OnMapInit(Entity<AlmanacBladeComponent> ent, ref MapInitEvent args)
    {
        if (TryComp<MeleeWeaponComponent>(ent, out var melee))
            Refresh((ent.Owner, ent.Comp, melee));
    }

    private void OnMeleeHit(Entity<AlmanacBladeComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit || args.HitEntities.Count == 0 || ent.Comp.LeapDay)
            return;

        if (ent.Comp.Afternoon)
        {
            foreach (var hit in args.HitEntities)
            {
                if (TryComp<FlammableComponent>(hit, out var flammable))
                    _flammable.AdjustFireStacks(hit, ent.Comp.FireStacks, flammable, ignite: true);
            }
        }

        if (!ent.Comp.Christmas)
            return;

        _audio.PlayPvs(ent.Comp.ChristmasSound, ent);

        if (_ores >= OresForPresents || _timing.CurTime < ent.Comp.NextPresent)
            return;

        ent.Comp.NextPresent = _timing.CurTime + ent.Comp.PresentCooldown;
        Spawn(ent.Comp.Present, Transform(args.User).Coordinates);
    }

    private void OnEquippedHand(Entity<AlmanacBladeComponent> ent, ref GotEquippedHandEvent args)
    {
        if (ent.Comp.AprilFoolsOver)
            Explode(ent);
    }

    private void Explode(Entity<AlmanacBladeComponent> ent)
    {
        _explosion.QueueExplosion(ent, ExplosionSystem.DefaultExplosionPrototypeId.Id, ent.Comp.ExplosionIntensity, 5f, 10f, canCreateVacuum: false);
        QueueDel(ent);
    }

    private void OnArtifactActivated(Entity<AlmanacBladeArtifactSpawnerComponent> ent, ref XenoArtifactNodeActivatedEvent args)
    {
        if (_harvests >= HarvestsForMaints || !_random.Prob(ent.Comp.Chance))
            return;

        Spawn(ent.Comp.Blade, args.Coordinates);
    }

    private void OnChestMapInit(Entity<AlmanacBladeChestSpawnerComponent> ent, ref MapInitEvent args)
    {
        if (!TryComp<EntityStorageComponent>(ent, out var storage)
            || _harvests >= HarvestsForMaints
            || !_random.Prob(ent.Comp.Chance))
            return;

        var blade = Spawn(ent.Comp.Blade, Transform(ent).Coordinates);
        _entityStorage.Insert(blade, ent, storage);
    }

    private void OnMaterialInserted(Entity<AlmanacOreCounterComponent> ent, ref MaterialEntityInsertedEvent args)
    {
        if (_tag.HasTag(args.Inserted, OreTag))
            _ores += args.Count;
    }

    private void OnPlantHarvested(ref PlantHarvestedEvent args)
    {
        _harvests++;

        if (_harvests != HarvestsForMaints || !_random.Prob(MaintsChance))
            return;

        var closets = new List<EntityUid>();
        var query = EntityQueryEnumerator<EntityStorageComponent, MetaDataComponent>();
        while (query.MoveNext(out var uid, out _, out var meta))
        {
            if (meta.EntityPrototype?.ID.StartsWith(MaintsClosetPrefix) == true)
                closets.Add(uid);
        }

        if (closets.Count == 0)
            return;

        var closet = _random.Pick(closets);
        var blade = Spawn(BladePrototype, Transform(closet).Coordinates);
        _entityStorage.Insert(blade, closet);
    }

    private void OnGetHiddenCargoProducts(ref GetHiddenCargoProductsEvent args)
    {
        var date = DateOnly.FromDateTime(DateTime.Now);
        var available = date.DayOfWeek == DayOfWeek.Sunday
            && AlmanacCalendar.IsFullMoon(DateTime.UtcNow)
            && _readied < MaxReadiedForCargo
            && _ticker.CurrentPreset is { } preset
            && CargoPresets.Contains(preset.ID);

        if (!available)
            args.Hidden.Add(CargoProduct);
    }

    private void OnJobsAssigned(RulePlayerJobsAssignedEvent args)
    {
        _readied = args.Players.Length;
    }

    private void OnRoundRestart(RoundRestartCleanupEvent args)
    {
        _flips = 0;
        _harvests = 0;
        _ores = 0;
        _readied = 0;
    }
}
