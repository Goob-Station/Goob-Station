using Content.Server.Botany.Components;
using Content.Shared._NF.PlantAnalyzer;
using Content.Server.PowerCell;
using Content.Shared.Atmos;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Item.ItemToggle.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using System.Linq;

namespace Content.Server.Botany.Systems;

public sealed partial class PlantAnalyzerSystem : EntitySystem
{
    [Dependency] private IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly PowerCellSystem _cell = default!;
    [Dependency] private ItemToggleSystem _toggle = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private UserInterfaceSystem _uiSystem = default!;
    [Dependency] private TransformSystem _transformSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PlantAnalyzerComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<PlantAnalyzerComponent, PlantAnalyzerDoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<PlantAnalyzerComponent, EntGotInsertedIntoContainerMessage>(OnInsertedIntoContainer);
        SubscribeLocalEvent<PlantAnalyzerComponent, ItemToggledEvent>(OnToggled);
        SubscribeLocalEvent<PlantAnalyzerComponent, DroppedEvent>(OnDropped);
        Subs.BuiEvents<PlantAnalyzerComponent>(PlantAnalyzerUiKey.Key, subs => { subs.Event<BoundUIClosedEvent>(OnPlantAnalyzerUiClosed); });
    }

    private TimeSpan _nextUpdate = TimeSpan.Zero;
    private TimeSpan _updateInterval = TimeSpan.FromSeconds(1);

    private void OnAfterInteract(Entity<PlantAnalyzerComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Target == null || !args.CanReach || !_cell.HasActivatableCharge(ent, user: args.User))
            return;

        if (ent.Comp.DoAfter != null)
            return;

        if (HasComp<SeedComponent>(args.Target) || TryComp<PlantHolderComponent>(args.Target, out var plantHolder) && plantHolder.Seed != null)
        {
            var doAfterArgs = new DoAfterArgs(EntityManager, args.User, ent.Comp.Settings.ScanDelay,
                new PlantAnalyzerDoAfterEvent(), ent, target: args.Target, used: ent)
            {
                NeedHand = true,
                BreakOnDamage = true,
                BreakOnMove = true,
                MovementThreshold = 0.01f
            };

            _doAfterSystem.TryStartDoAfter(doAfterArgs, out ent.Comp.DoAfter);

        }
    }

    private void OnDoAfter(Entity<PlantAnalyzerComponent> ent, ref PlantAnalyzerDoAfterEvent args)
    {
        ent.Comp.DoAfter = null;

        if (args.Handled || args.Cancelled || args.Args.Target == null)
            return;

        _audio.PlayPvs(ent.Comp.ScanningEndSound, ent);

        ent.Comp.ScannedEntity = args.Args.Target.Value;
        _nextUpdate = TimeSpan.Zero;

        _toggle.TryActivate(ent.Owner);
        OpenUserInterface(args.User, ent);

        args.Handled = true;
    }

    private void OnPlantAnalyzerUiClosed(EntityUid uid, PlantAnalyzerComponent comp, BoundUIClosedEvent args)
    {
        if (!args.UiKey.Equals(PlantAnalyzerUiKey.Key))
            return;

        if (!_uiSystem.IsUiOpen(uid, PlantAnalyzerUiKey.Key))
            _toggle.TryDeactivate(uid);
    }

    private void OnInsertedIntoContainer(Entity<PlantAnalyzerComponent> uid, ref EntGotInsertedIntoContainerMessage args)
    {
        if (uid.Comp.ScannedEntity is { } target)
            _toggle.TryDeactivate(uid.Owner);
    }

    private void OnDropped(Entity<PlantAnalyzerComponent> uid, ref DroppedEvent args)
    {
        if (uid.Comp.ScannedEntity is { } target)
            _toggle.TryDeactivate(uid.Owner);
    }

    private void OnToggled(Entity<PlantAnalyzerComponent> uid, ref ItemToggledEvent args)
    {
        if (!args.Activated && uid.Comp.ScannedEntity is { } target)
            StopAnalyzingEntity(uid, target);
    }


    private void OpenUserInterface(EntityUid user, EntityUid analyzer)
    {
        if (!TryComp<ActorComponent>(user, out var actor) || !_uiSystem.HasUi(analyzer, PlantAnalyzerUiKey.Key))
            return;

        _uiSystem.OpenUi(analyzer, PlantAnalyzerUiKey.Key, actor.PlayerSession);
    }
    private void StopAnalyzingEntity(Entity<PlantAnalyzerComponent> ent, EntityUid target)
    {
        ent.Comp.ScannedEntity = null;
        _uiSystem.CloseUi(ent.Owner, PlantAnalyzerUiKey.Key);
        _toggle.TryDeactivate(ent.Owner);
    }

    public override void Update(float frameTime)
    {

        if (_nextUpdate < _updateInterval)
        {
            _nextUpdate += TimeSpan.FromSeconds(frameTime);
            return;
        }

        var query = EntityQueryEnumerator<PlantAnalyzerComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var comp, out var analyzer))
        {
            if (comp.ScannedEntity is not { } target)
                continue;

            if (!HasComp<SeedComponent>(target) && !HasComp<PlantHolderComponent>(target) ||
                !_transformSystem.InRange(Transform(target).Coordinates, analyzer.Coordinates, comp.MaxScanRange))
            {
                StopAnalyzingEntity((uid, comp), target);
                continue;
            }

            UpdateScannedUser((uid, comp), target);
        }
        _nextUpdate -= _updateInterval;
    }


    public void UpdateScannedUser(Entity<PlantAnalyzerComponent> ent, EntityUid target)
    {
        if (!_uiSystem.HasUi(ent, PlantAnalyzerUiKey.Key))
            return;

        if (TryComp<SeedComponent>(target, out var seedComp))
        {
            if (seedComp.Seed != null)
            {
                var state = ObtainingGeneDataSeed(seedComp.Seed, target, false);
                _uiSystem.ServerSendUiMessage(ent.Owner, PlantAnalyzerUiKey.Key, state);
            }
            else if (seedComp.SeedId != null && _prototypeManager.TryIndex(seedComp.SeedId, out SeedPrototype? protoSeed))
            {
                var state = ObtainingGeneDataSeed(protoSeed, target, false);
                _uiSystem.ServerSendUiMessage(ent.Owner, PlantAnalyzerUiKey.Key, state);
            }
        }
        else if (TryComp<PlantHolderComponent>(target, out var plantComp))
        {
            if (plantComp.Seed != null)
            {
                var state = ObtainingGeneDataSeed(plantComp.Seed, target, true);
                _uiSystem.ServerSendUiMessage(ent.Owner, PlantAnalyzerUiKey.Key, state);
            }
        }
    }

    /// <summary>
    ///     Analysis of seed from prototype.
    /// </summary>
    public PlantAnalyzerScannedSeedPlantInformation ObtainingGeneDataSeed(SeedData seedData, EntityUid target, bool isTray)
    {
        // Get trickier fields first.
        AnalyzerHarvestType harvestType = AnalyzerHarvestType.Unknown;
        switch (seedData.HarvestRepeat)
        {
            case HarvestType.Repeat:
                harvestType = AnalyzerHarvestType.Repeat;
                break;
            case HarvestType.NoRepeat:
                harvestType = AnalyzerHarvestType.NoRepeat;
                break;
            case HarvestType.SelfHarvest:
                harvestType = AnalyzerHarvestType.SelfHarvest;
                break;
            default:
                break;
        }

        var mutationProtos = seedData.MutationPrototypes;
        List<string> mutationStrings = new();
        foreach (var mutationProto in mutationProtos)
        {
            if (_prototypeManager.TryIndex<SeedPrototype>(mutationProto, out var seed))
            {
                mutationStrings.Add(seed.DisplayName);
            }
        }

        PlantAnalyzerScannedSeedPlantInformation ret = new()
        {
            TargetEntity = GetNetEntity(target),
            IsTray = isTray,
            SeedName = seedData.DisplayName,
            SeedChem = seedData.Chemicals.Keys.ToArray(),
            HarvestType = harvestType,
            ExudeGases = GetGasFlags(seedData.ExudeGasses.Keys),
            ConsumeGases = GetGasFlags(seedData.ConsumeGasses.Keys),
            Endurance = seedData.Endurance,
            SeedYield = seedData.Yield,
            Lifespan = seedData.Lifespan,
            Maturation = seedData.Maturation,
            Production = seedData.Production,
            GrowthStages = seedData.GrowthStages,
            SeedPotency = seedData.Potency,
            Speciation = mutationStrings.ToArray(),
            NutrientConsumption = seedData.NutrientConsumption,
            WaterConsumption = seedData.WaterConsumption,
            IdealHeat = seedData.IdealHeat,
            HeatTolerance = seedData.HeatTolerance,
            IdealLight = seedData.IdealLight,
            LightTolerance = seedData.LightTolerance,
            ToxinsTolerance = seedData.ToxinsTolerance,
            LowPressureTolerance = seedData.LowPressureTolerance,
            HighPressureTolerance = seedData.HighPressureTolerance,
            PestTolerance = seedData.PestTolerance,
            WeedTolerance = seedData.WeedTolerance,
            Mutations = GetMutationFlags(seedData)
        };

        return ret;
    }

    public MutationFlags GetMutationFlags(SeedData plant)
    {
        MutationFlags ret = MutationFlags.None;
        if (plant.TurnIntoKudzu) ret |= MutationFlags.TurnIntoKudzu;
        if (plant.Seedless) ret |= MutationFlags.Seedless;
        if (plant.Ligneous) ret |= MutationFlags.Ligneous;
        if (plant.CanScream) ret |= MutationFlags.CanScream;
        if (!plant.Viable) ret |= MutationFlags.Unviable;

        return ret;
    }

    public GasFlags GetGasFlags(IEnumerable<Gas> gases)
    {
        var gasFlags = GasFlags.None;
        foreach (var gas in gases)
        {
            switch (gas)
            {
                case Gas.Nitrogen:
                    gasFlags |= GasFlags.Nitrogen;
                    break;
                case Gas.Oxygen:
                    gasFlags |= GasFlags.Oxygen;
                    break;
                case Gas.CarbonDioxide:
                    gasFlags |= GasFlags.CarbonDioxide;
                    break;
                case Gas.Plasma:
                    gasFlags |= GasFlags.Plasma;
                    break;
                case Gas.Tritium:
                    gasFlags |= GasFlags.Tritium;
                    break;
                case Gas.WaterVapor:
                    gasFlags |= GasFlags.WaterVapor;
                    break;
                case Gas.Ammonia:
                    gasFlags |= GasFlags.Ammonia;
                    break;
                case Gas.NitrousOxide:
                    gasFlags |= GasFlags.NitrousOxide;
                    break;
                case Gas.Frezon:
                    gasFlags |= GasFlags.Frezon;
                    break;
            }
        }
        return gasFlags;
    }

}
