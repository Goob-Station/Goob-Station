using System.Threading;
using System.Threading.Tasks;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Asynchronous;
using Robust.Shared.Timing;
using Content.Server.Database;
using Robust.Server.Player;
using Content.Shared._TBDStation.ServerKarma.Events;
using Content.Server.Administration.Commands;
using Content.Server.Administration.Components;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Server.Electrocution;
using Content.Server.Explosion.EntitySystems;
using Content.Server.GhostKick;
using Content.Server.Medical;
using Content.Server.Nutrition.EntitySystems;
using Content.Server.Pointing.Components;
using Content.Server.Polymorph.Systems;
using Content.Server.Popups;
using Content.Server.Speech.Components;
using Content.Server.Storage.Components;
using Content.Server.Storage.EntitySystems;
using Content.Server.Tabletop;
using Content.Server.Tabletop.Components;
using Content.Server.Temperature.Components;
using Content.Shared._Goobstation.Wizard.Traps;
using Content.Shared.Administration;
using Content.Shared.Administration.Components;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using static Robust.Shared.GameObjects.EntitySystem;
using Content.Shared.Clumsy;
using Content.Shared.Clothing.Components;
using Content.Shared.Cluwne;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Database;
using Content.Shared.Electrocution;
using Content.Shared.Interaction.Components;
using Content.Shared.Inventory;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Popups;
using Content.Shared.Slippery;
using Content.Shared.Tabletop.Components;
using Content.Shared.Tools.Systems;
using Content.Shared.Verbs;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using Timer = Robust.Shared.Timing.Timer;
using Content.Server.Speech.EntitySystems;
using Content.Shared.Speech.Components;
using Content.Server.Administration.Systems;
using Content.Shared.Administration.Logs;
using Content.Server.Administration.Logs;
using Content.Server.Administration.Managers;
using Content.Server.Administration.UI;
using Content.Server.Disposal.Tube;
using Content.Server.Disposal.Tube.Components;
using Content.Server.EUI;
using Content.Server.GameTicking;
using Content.Server.Ghost.Roles;
using Content.Server.Mind;
using Content.Server.Mind.Commands;
using Content.Server.Prayer;
using Content.Server.Station.Systems;
using Content.Server.Xenoarchaeology.XenoArtifacts;
using Content.Server.Xenoarchaeology.XenoArtifacts.Triggers.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Configurable;
using Content.Shared.Examine;
using Content.Shared.GameTicking;
using Content.Shared.Mind.Components;
using Robust.Server.Console;
using Robust.Server.GameObjects;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;
using Robust.Shared.Toolshed;
using System.Linq;
using Content.Server.Silicons.Laws;
using Content.Shared.Silicons.Laws.Components;
using Content.Shared.Silicons.StationAi;
using static Content.Shared.Configurable.ConfigurationComponent;

namespace Content.Server._TBDStation.ServerKarma;
public sealed partial class KarmaPunishmentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstreamSystem = default!;
    [Dependency] private readonly BodySystem _bodySystem = default!;
    [Dependency] private readonly CreamPieSystem _creamPieSystem = default!;
    [Dependency] private readonly ElectrocutionSystem _electrocutionSystem = default!;
    [Dependency] private readonly EntityStorageSystem _entityStorageSystem = default!;
    [Dependency] private readonly ExplosionSystem _explosionSystem = default!;
    [Dependency] private readonly FixtureSystem _fixtures = default!;
    [Dependency] private readonly FlammableSystem _flammableSystem = default!;
    [Dependency] private readonly GhostKickManager _ghostKickManager = default!;
    [Dependency] private readonly SharedGodmodeSystem _sharedGodmodeSystem = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifierSystem = default!;
    [Dependency] private readonly PolymorphSystem _polymorphSystem = default!;
    [Dependency] private readonly MobThresholdSystem _mobThresholdSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly TabletopSystem _tabletopSystem = default!;
    [Dependency] private readonly VomitSystem _vomitSystem = default!;
    [Dependency] private readonly WeldableSystem _weldableSystem = default!;
    [Dependency] private readonly SharedContentEyeSystem _eyeSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly SuperBonkSystem _superBonkSystem = default!;
    [Dependency] private readonly SlipperySystem _slipperySystem = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly ServerKarmaManager _karmaMan = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    // public void PostInject()
    // {
    // }
    public override void Initialize()
    {
        base.Initialize();
        _karmaMan.KarmaChange += DeterminePunishment;
    }
    public override void Shutdown()
    {
        base.Shutdown();
        _karmaMan.KarmaChange -= DeterminePunishment;
    }

    private void DeterminePunishment(PlayerKarmaChangeEvent ev)
    {
        if (!ev.UserSes.AttachedEntity.HasValue)
            return;
        EntityUid player = ev.UserSes.AttachedEntity.Value;
        switch (ev.NewKarma)
        {
            case > 0:
                return;
            case > -200 and <= 0:
                break;
            case > -600 and <= -200:
                break;
            case > -850 and <= -600:
                break;
            case > -1000 and <= -850:
                Harsh(player);
                break;
            case < -1100:
                Kill(player);
                break;
            default:
                break;
        }
    }

    internal void Harsh(EntityUid player)
    {
        var i = 2;
        i /= 2;
    }

    private void Kill(EntityUid target)
    {
        if (!EntityManager.TryGetComponent(target, out ActorComponent? actor))
            return;

        var player = actor.PlayerSession;
        // 1984.
        if (HasComp<MapComponent>(target) || HasComp<MapGridComponent>(target))
            return;

        bool got_smitted = false;
        int attempts = 0;
        while (!got_smitted && attempts++ < 9)
        {
            int i = _random.Next(38);
            AnySmite(i, target, ref got_smitted);
        }
    }
    private void AnySmite(int i, EntityUid target, ref bool got_smitted)
    {
        switch (i)
        {
            case 0:
                var coords = _transformSystem.GetMapCoordinates(target);
                Timer.Spawn(_gameTiming.TickPeriod,
                    () => _explosionSystem.QueueExplosion(coords, ExplosionSystem.DefaultExplosionPrototypeId,
                        4, 1, 2, target, maxTileBreak: 0), // it gibs, damage doesn't need to be high.
                    CancellationToken.None);

                _bodySystem.GibBody(target);
                got_smitted = true;
                break;
            case 1:
                if (TryComp<FlammableComponent>(target, out var flammable))
                {
                    // Fuck you. Burn Forever.
                    flammable.FireStacks = flammable.MaximumFireStacks;
                    _flammableSystem.Ignite(target, target);
                    var xform5 = Transform(target);
                    _popupSystem.PopupEntity(Loc.GetString("admin-smite-set-alight-self"), target,
                        target, PopupType.LargeCaution);
                    _popupSystem.PopupCoordinates(Loc.GetString("admin-smite-set-alight-others", ("name", target)), xform5.Coordinates,
                        Filter.PvsExcept(target), true, PopupType.MediumCaution);
                }
                got_smitted = true;
                break;
            case 2:
                _polymorphSystem.PolymorphEntity(target, "AdminMonkeySmite");
                got_smitted = true;
                break;
            case 3:
                _polymorphSystem.PolymorphEntity(target, "AdminDisposalsSmite");
                got_smitted = true;
                break;
            case 4:
                if (TryComp<DamageableComponent>(target, out var damageable) &&
                    HasComp<MobStateComponent>(target))
                {
                    int damageToDeal;
                    if (!_mobThresholdSystem.TryGetThresholdForState(target, MobState.Critical, out var criticalThreshold)) {
                        // We can't crit them so try killing them.
                        if (!_mobThresholdSystem.TryGetThresholdForState(target, MobState.Dead,
                                out var deadThreshold))
                            return;// whelp.
                        damageToDeal = deadThreshold.Value.Int() - (int) damageable.TotalDamage;
                    }
                    else
                    {
                        damageToDeal = criticalThreshold.Value.Int() - (int) damageable.TotalDamage;
                    }

                    if (damageToDeal <= 0)
                        damageToDeal = 100; // murder time.

                    if (_inventorySystem.TryGetSlots(target, out var slotDefinitions))
                    {
                        foreach (var slot in slotDefinitions)
                        {
                            if (!_inventorySystem.TryGetSlotEntity(target, slot.Name, out var slotEnt))
                                continue;

                            RemComp<InsulatedComponent>(slotEnt.Value); // Fry the gloves.
                        }
                    }

                    _electrocutionSystem.TryDoElectrocution(target, null, damageToDeal,
                        TimeSpan.FromSeconds(30), refresh: true, ignoreInsulation: true);
                }
                got_smitted = true;
                break;
            case 5:
                if (TryComp<CreamPiedComponent>(target, out var creamPied))
                {
                    _creamPieSystem.SetCreamPied(target, creamPied, true);
                }
                got_smitted = true;
                break;
            case 6:
                if (TryComp<BloodstreamComponent>(target, out var bloodstream))
                {
                    _bloodstreamSystem.SpillAllSolutions(target, bloodstream);
                    var xform4 = Transform(target);
                    _popupSystem.PopupEntity(Loc.GetString("admin-smite-remove-blood-self"), target,
                        target, PopupType.LargeCaution);
                    _popupSystem.PopupCoordinates(Loc.GetString("admin-smite-remove-blood-others", ("name", target)), xform4.Coordinates,
                        Filter.PvsExcept(target), true, PopupType.MediumCaution);
                }
                got_smitted = true;
                break;
            case 7:
                if (TryComp<BodyComponent>(target, out var body))
                {
                    _vomitSystem.Vomit(target, -1000, -1000); // You feel hollow!
                    var organs = _bodySystem.GetBodyOrganEntityComps<TransformComponent>((target, body));
                    var baseXform = Transform(target);
                    foreach (var organ in organs)
                    {
                        if (HasComp<BrainComponent>(organ.Owner) || HasComp<EyeComponent>(organ.Owner))
                            continue;

                        _transformSystem.PlaceNextTo((organ.Owner, organ.Comp1), (target, baseXform));
                    }

                    _popupSystem.PopupEntity(Loc.GetString("admin-smite-vomit-organs-self"), target,
                        target, PopupType.LargeCaution);
                    _popupSystem.PopupCoordinates(Loc.GetString("admin-smite-vomit-organs-others", ("name", target)), baseXform.Coordinates,
                        Filter.PvsExcept(target), true, PopupType.MediumCaution);
                }
                got_smitted = true;
                break;
            case 8:
                var baseXform2 = Transform(target);
                foreach (var part in _bodySystem.GetBodyChildrenOfType(target, BodyPartType.Hand))
                {
                    _transformSystem.AttachToGridOrMap(part.Id);
                }
                _popupSystem.PopupEntity(Loc.GetString("admin-smite-remove-hands-self"), target,
                    target, PopupType.LargeCaution);
                _popupSystem.PopupCoordinates(Loc.GetString("admin-smite-remove-hands-other", ("name", target)), baseXform2.Coordinates,
                    Filter.PvsExcept(target), true, PopupType.Medium);
                got_smitted = true;
                break;
            case 9:
                if (TryComp<BodyComponent>(target, out var body1))
                {
                    var baseXform1 = Transform(target);
                    foreach (var part in _bodySystem.GetBodyChildrenOfType(target, BodyPartType.Hand, body1))
                    {
                        _transformSystem.AttachToGridOrMap(part.Id);
                        break;
                    }
                    _popupSystem.PopupEntity(Loc.GetString("admin-smite-remove-hands-self"), target,
                        target, PopupType.LargeCaution);
                    _popupSystem.PopupCoordinates(Loc.GetString("admin-smite-remove-hands-other", ("name", target)), baseXform1.Coordinates,
                        Filter.PvsExcept(target), true, PopupType.Medium);
                }
                got_smitted = true;
                break;
            case 10:
                if (TryComp<BodyComponent>(target, out var body2))
                {
                    foreach (var entity in _bodySystem.GetBodyOrganEntityComps<StomachComponent>((target, body2)))
                    {
                        QueueDel(entity.Owner);
                    }

                    _popupSystem.PopupEntity(Loc.GetString("admin-smite-stomach-removal-self"), target,
                        target, PopupType.LargeCaution);
                }
                got_smitted = true;
                break;
            case 11:
                if (TryComp<BodyComponent>(target, out var body3))
                {
                    foreach (var entity in _bodySystem.GetBodyOrganEntityComps<LungComponent>((target, body3)))
                    {
                        QueueDel(entity.Owner);
                    }

                    _popupSystem.PopupEntity(Loc.GetString("admin-smite-lung-removal-self"), target,
                        target, PopupType.LargeCaution);
                }
                got_smitted = true;
                break;
            case 12:
                if (TryComp<PhysicsComponent>(target, out var physics))
                {
                    var xform3 = Transform(target);
                    var fixtures = Comp<FixturesComponent>(target);
                    xform3.Anchored = false; // Just in case.
                    _physics.SetBodyType(target, BodyType.Dynamic, manager: fixtures, body: physics);
                    _physics.SetBodyStatus(target, physics, BodyStatus.InAir);
                    _physics.WakeBody(target, manager: fixtures, body: physics);

                    foreach (var fixture in fixtures.Fixtures.Values)
                    {
                        if (!fixture.Hard)
                            continue;

                        _physics.SetRestitution(target, fixture, 1.1f, false, fixtures);
                    }

                    _fixtures.FixtureUpdate(target, manager: fixtures, body: physics);

                    _physics.SetLinearVelocity(target, _random.NextVector2(1.5f, 1.5f), manager: fixtures, body: physics);
                    _physics.SetAngularVelocity(target, MathF.PI * 12, manager: fixtures, body: physics);
                    _physics.SetLinearDamping(target, physics, 0f);
                    _physics.SetAngularDamping(target, physics, 0f);
                }
                got_smitted = true;
                break;
            case 13:
                if (TryComp<PhysicsComponent>(target, out physics))
                {
                    var xform2 = Transform(target);
                    var fixtures = Comp<FixturesComponent>(target);
                    xform2.Anchored = false; // Just in case.

                    _physics.SetBodyType(target, BodyType.Dynamic, body: physics);
                    _physics.SetBodyStatus(target, physics, BodyStatus.InAir);
                    _physics.WakeBody(target, manager: fixtures, body: physics);

                    foreach (var fixture in fixtures.Fixtures.Values)
                    {
                        _physics.SetHard(target, fixture, false, manager: fixtures);
                    }

                    _physics.SetLinearVelocity(target, _random.NextVector2(8.0f, 8.0f), manager: fixtures, body: physics);
                    _physics.SetAngularVelocity(target, MathF.PI * 12, manager: fixtures, body: physics);
                    _physics.SetLinearDamping(target, physics, 0f);
                    _physics.SetAngularDamping(target, physics, 0f);
                }
                got_smitted = true;
                break;
            case 14:
                _polymorphSystem.PolymorphEntity(target, "AdminBreadSmite");
                got_smitted = true;
                break;
            case 15:
                _polymorphSystem.PolymorphEntity(target, "AdminMouseSmite");
                got_smitted = true;
                break;
            // case 16: // Simply too mean
            //     if (TryComp<ActorComponent>(target, out var actorComponent))
            //     {
            //         _ghostKickManager.DoDisconnect(actorComponent.PlayerSession.Channel, "Smitten.");
            //     }
            //     got_smitted = true;
            //     break;
            case 17:
                if (HasComp<TemperatureComponent>(target))
                    EnsureComp<IceCubeComponent>(target);
                got_smitted = true;
                break;
            case 18:
                if (TryComp<InventoryComponent>(target, out var inventory))
                {
                    var ears = Spawn("ClothingHeadHatCatEars", Transform(target).Coordinates);
                    EnsureComp<UnremoveableComponent>(ears);
                    _inventorySystem.TryUnequip(target, "head", true, true, false, inventory);
                    _inventorySystem.TryEquip(target, ears, "head", true, true, false, inventory);
                }
                got_smitted = true;
                break;
            case 19:
                EnsureComp<KillSignComponent>(target);
                got_smitted = true;
                break;
            case 20:
                EnsureComp<CluwneComponent>(target);
                got_smitted = true;
                break;
            case 21:
                SetOutfitCommand.SetOutfit(target, "JanitorMaidGear", EntityManager, (_, clothing) =>
                {
                    if (HasComp<ClothingComponent>(clothing))
                        EnsureComp<UnremoveableComponent>(clothing);
                    EnsureComp<ClumsyComponent>(target);
                });
                got_smitted = true;
                break;
            case 22:
                EnsureComp<PointingArrowAngeringComponent>(target);
                got_smitted = true;
                break;
            case 23:
                EntityManager.QueueDeleteEntity(target);
                Spawn("Ash", Transform(target).Coordinates);
                _popupSystem.PopupEntity(Loc.GetString("admin-smite-turned-ash-other", ("name", target)), target, PopupType.LargeCaution);
                got_smitted = true;
                break;
            case 24:
                EnsureComp<BufferingComponent>(target);
                got_smitted = true;
                break;
            case 25:
                _polymorphSystem.PolymorphEntity(target, "AdminInstrumentSmite");
                got_smitted = true;
                break;
            case 26:
                var grav = EnsureComp<MovementIgnoreGravityComponent>(target);
                grav.Weightless = true;

                Dirty(target, grav);
                got_smitted = true;
                break;
            case 27:
                _polymorphSystem.PolymorphEntity(target, "AdminLizardSmite");
                got_smitted = true;
                break;
            case 28:
                var xform = Transform(target);
                var locker = Spawn("ClosetMaintenance", xform.Coordinates);
                if (TryComp<EntityStorageComponent>(locker, out var storage))
                {
                    _entityStorageSystem.ToggleOpen(target, locker, storage);
                    _entityStorageSystem.Insert(target, locker, storage);
                    _entityStorageSystem.ToggleOpen(target, locker, storage);
                }
                _weldableSystem.SetWeldedState(locker, true);
                got_smitted = true;
                break;
            case 29:
                EnsureComp<HeadstandComponent>(target);
                got_smitted = true;
                break;
            case 30:
                var eye = EnsureComp<ContentEyeComponent>(target);
                _eyeSystem.SetZoom(target, eye.TargetZoom * -1, ignoreLimits: true);
                got_smitted = true;
                break;
            case 31:
                var movementSpeed = EnsureComp<MovementSpeedModifierComponent>(target);
                (movementSpeed.BaseSprintSpeed, movementSpeed.BaseWalkSpeed) = (movementSpeed.BaseWalkSpeed, movementSpeed.BaseSprintSpeed);

                Dirty(target, movementSpeed);

                _popupSystem.PopupEntity(Loc.GetString("admin-smite-run-walk-swap-prompt"), target,
                    target, PopupType.LargeCaution);
                got_smitted = true;
                break;
            case 32:
                EnsureComp<BackwardsAccentComponent>(target);
                got_smitted = true;
                break;
            case 33:
                EnsureComp<DisarmProneComponent>(target);
                got_smitted = true;
                break;
            case 34:
                movementSpeed = EnsureComp<MovementSpeedModifierComponent>(target);
                _movementSpeedModifierSystem?.ChangeBaseSpeed(target, 400, 8000, 40, movementSpeed);

                _popupSystem.PopupEntity(Loc.GetString("admin-smite-super-speed-prompt"), target,
                    target, PopupType.LargeCaution);
                got_smitted = true;
                break;
            case 35:
                _superBonkSystem.StartSuperBonk(target, stopWhenDead: true);
                got_smitted = true;
                break;
            case 36:
                _superBonkSystem.StartSuperBonk(target);
                got_smitted = true;
                break;
            case 37:
                var hadSlipComponent = EnsureComp(target, out SlipperyComponent slipComponent);
                if (!hadSlipComponent)
                {
                    slipComponent.SuperSlippery = true;
                    slipComponent.ParalyzeTime = 5;
                    slipComponent.LaunchForwardsMultiplier = 20;
                }

                _slipperySystem.TrySlip(target, slipComponent, target, requiresContact: false);
                if (!hadSlipComponent)
                {
                    RemComp(target, slipComponent);
                }
                got_smitted = true;
                break;
            case 38:
                EnsureComp<BarkAccentComponent>(target);
                EnsureComp<BleatingAccentComponent>(target);
                EnsureComp<FrenchAccentComponent>(target);
                EnsureComp<GermanAccentComponent>(target);
                EnsureComp<LizardAccentComponent>(target);
                EnsureComp<MobsterAccentComponent>(target);
                EnsureComp<MothAccentComponent>(target);
                EnsureComp<OwOAccentComponent>(target);
                EnsureComp<SkeletonAccentComponent>(target);
                EnsureComp<SouthernAccentComponent>(target);
                EnsureComp<SpanishAccentComponent>(target);
                EnsureComp<StutteringAccentComponent>(target);
                EnsureComp<MedievalAccentComponent>(target); // Goobtation
                EnsureComp<MaoistAccentComponent>(target); // Goobtation
                EnsureComp<OhioAccentComponent>(target); // Goobtation
                EnsureComp<PirateAccentComponent>(target); // Goobtation
                EnsureComp<VulgarAccentComponent>(target); // Goobtation

                if (_random.Next(0, 8) == 0)
                {
                    EnsureComp<BackwardsAccentComponent>(target); // was asked to make this at a low chance idk
                }
                got_smitted = true;
                break;
            default:
                got_smitted = true;
                break;
        }
        if (got_smitted)
            _adminLogger.Add(LogType.Karma,
            LogImpact.Medium,
            $"{ToPrettyString(target):actor} got smitted by AnySmite({i}) from too much karma loss.");
    }

        // SmiteKill();
    //     int i = _rnd.Next(10);
    //     switch (i)
    //     {
    //         case 0:
    //             var coords = _transformSystem.GetMapCoordinates(target);
    //             Timer.Spawn(_gameTiming.TickPeriod,
    //                 () => _explosionSystem.QueueExplosion(coords, ExplosionSystem.DefaultExplosionPrototypeId,
    //                     4, 1, 2, target, maxTileBreak: 0), // it gibs, damage doesn't need to be high.
    //                 CancellationToken.None);

    //             _bodySystem.GibBody(target);
    //             break;
    //         case 1:
    //             _polymorphSystem.PolymorphEntity(target, "AdminMonkeySmite");
    //             break;
    //         case 2:
    //             _polymorphSystem.PolymorphEntity(target, "AdminDisposalsSmite");
    //             break;
    //         case 3:
    //             _polymorphSystem.PolymorphEntity(target, "AdminBreadSmite");
    //             break;
    //         case 4:
    //             _polymorphSystem.PolymorphEntity(target, "AdminMouseSmite");
    //             break;
    //         case 5:
    //             EnsureComp<PointingArrowAngeringComponent>(target);
    //             break;
    //         case 6:
    //             if (TryComp<FlammableComponent>(target, out var flammable))
    //             {
    //                 // Fuck you. Burn Forever.
    //                 flammable.FireStacks = flammable.MaximumFireStacks;
    //                 _flammableSystem.Ignite(target, args.User);
    //                 var xform = Transform(target);
    //                 _popupSystem.PopupEntity(Loc.GetString("admin-smite-set-alight-self"), target,
    //                     target, PopupType.LargeCaution);
    //                 _popupSystem.PopupCoordinates(Loc.GetString("admin-smite-set-alight-others", ("name", target)), xform.Coordinates,
    //                     Filter.PvsExcept(target), true, PopupType.MediumCaution);
    //             }
    //             break;
    //         case 7:
    //             if (TryComp<DamageableComponent>(target, out var damageable) &&
    //                 HasComp<MobStateComponent>(target))
    //             {
    //                 int damageToDeal;
    //                 if (!_mobThresholdSystem.TryGetThresholdForState(target, MobState.Critical, out var criticalThreshold)) {
    //                     // We can't crit them so try killing them.
    //                     if (!_mobThresholdSystem.TryGetThresholdForState(target, MobState.Dead,
    //                             out var deadThreshold))
    //                         return;// whelp.
    //                     damageToDeal = deadThreshold.Value.Int() - (int) damageable.TotalDamage;
    //                 }
    //                 else
    //                 {
    //                     damageToDeal = criticalThreshold.Value.Int() - (int) damageable.TotalDamage;
    //                 }

    //                 if (damageToDeal <= 0)
    //                     damageToDeal = 100; // murder time.

    //                 if (_inventorySystem.TryGetSlots(target, out var slotDefinitions))
    //                 {
    //                     foreach (var slot in slotDefinitions)
    //                     {
    //                         if (!_inventorySystem.TryGetSlotEntity(target, slot.Name, out var slotEnt))
    //                             continue;

    //                         RemComp<InsulatedComponent>(slotEnt.Value); // Fry the gloves.
    //                     }
    //                 }

    //                 _electrocutionSystem.TryDoElectrocution(target, null, damageToDeal,
    //                     TimeSpan.FromSeconds(30), refresh: true, ignoreInsulation: true);
    //             }
    //             break;
    //         case 8:
    //             if (TryComp<CreamPiedComponent>(target, out var creamPied))
    //             {
    //                 Text = creamPieName,
    //                 Category = VerbCategory.Smite,
    //                 Icon = new SpriteSpecifier.Rsi(new ("/Textures/Objects/Consumable/Food/Baked/pie.rsi"), "plain-slice"),
    //                 Act = () =>
    //                 {
    //                     _creamPieSystem.SetCreamPied(target, creamPied, true);
    //                 },
    //                 Impact = LogImpact.Extreme,
    //                 Message = string.Join(": ", creamPieName, Loc.GetString("admin-smite-creampie-description"))
    //             }
    //             break;
    //         case 9:
    //             break;
    //     }
}
