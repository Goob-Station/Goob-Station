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
using Content.Server.Temperature.Components;
using Content.Shared._Goobstation.Wizard.Traps;
using Content.Shared.Administration.Components;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
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
using Content.Shared.Tools.Systems;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Random;
using Timer = Robust.Shared.Timing.Timer;
using Content.Shared.Speech.Components;
using Content.Server.Administration.Systems;
using Content.Shared.Administration.Logs;
using Content.Server.Chat.Managers;

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
    [Dependency] private readonly IChatManager _chatManager = default!;
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
        EntityUid target = ev.UserSes.AttachedEntity.Value;
        // Wheights
        int nothing = 0, bitter = 0, harsh = 0, nasty = 0, harm = 0, kill = 0;
        switch (ev.NewKarma)
        {
            case > 0:
                return;
            case > -200 and <= 0:
                nothing = 90;
                bitter = 10;
                break;
            case > -600 and <= -200:
                nothing = 75;
                bitter = 15;
                harsh = 5;
                harm = 5;
                break;
            case > -850 and <= -600:
                nothing = 60;
                bitter = 15;
                harsh = 10;
                nasty = 2;
                harm = 10;
                kill = 3;
                break;
            case > -1000 and <= -850:
                nothing = 40;
                bitter = 15;
                harsh = 15;
                nasty = 10;
                harm = 10;
                kill = 10;
                break;
            case > -1200 and <= -1100:
                nothing = 40;
                bitter = 20;
                harsh = 20;
                harm = 10;
                kill = 10;
                break;
            case <= -1200:
                nothing = 25;
                harsh = 25;
                nasty = 25;
                kill = 25;
                break;
            default:
                break;
        }
        if (!EntityManager.TryGetComponent(target, out ActorComponent? actor))
            return;

        var player = actor.PlayerSession;
        // 1984.
        if (HasComp<MapComponent>(target) || HasComp<MapGridComponent>(target))
            return;

        int totalWheight = nothing + bitter + harsh + nasty + harm + kill;
        int attempts = 0, i = 0;
        bool got_smitted = false;
        while (!got_smitted && attempts++ < 9)
        {
            i = _random.Next(totalWheight);
            if (i < nothing)
                return;
            i -= nothing;
            if (i < bitter)
                AnyBitterSmite(i, target, ref got_smitted);
            i -= bitter;
            if (i < harsh)
                AnyHarshSmite(i, target, ref got_smitted);
            i -= harsh;
            if (i < nasty)
                AnyNastySmite(i, target, ref got_smitted);
            i -= nasty;
            if (i < harm)
                AnyHarmSmite(i, target, ref got_smitted);
            i -= harm;
            if (i < kill)
                AnyKillSmite(i, target, ref got_smitted);
            i -= kill;
        }

        if (got_smitted)
        {
            // _popupSystem.PopupEntity("Your actions have consequences!", target, target, PopupType.LargeCaution); // Don't popup since SOME punishments have some popups.
            _chatManager.DispatchServerMessage(player, "Your actions have consequences!", true);
            _adminLogger.Add(LogType.Karma,
                LogImpact.High,
                $"{ToPrettyString(target):actor} got automatically smitted by {i}(AnyLevelSmite). from too much karma loss.");
        }
    }

    #region Bitter
    /// Stuff that hardly sucks and is easily fixable
    private void AnyBitterSmite(int i, EntityUid target, ref bool got_smitted)
    {
        i = _random.Next(6);
        switch (i)
        {
            case 0:
                if (TryComp<CreamPiedComponent>(target, out var creamPied))
                {
                    _creamPieSystem.SetCreamPied(target, creamPied, true);
                    got_smitted = true;
                }
                break;
            case 1:
                if (HasComp<TemperatureComponent>(target))
                {
                    EnsureComp<IceCubeComponent>(target);
                    got_smitted = true;
                }
                break;
            case 2:
                got_smitted = BitterSlip(target);
                break;
            case 3:
                got_smitted = BitterSlow(target);
                break;
            case 4:
                got_smitted = BitterSpeakBackwards(target);
                break;
            case 5:
                got_smitted = BitterZoom(target);
                break;
            default:
                break;
        }
    }

    private bool BitterZoom(EntityUid target)
    {
        bool got_smitted;
        var eye = EnsureComp<ContentEyeComponent>(target);
        _eyeSystem.SetZoom(target, eye.TargetZoom * -1, ignoreLimits: true);
        got_smitted = true;
        return got_smitted;
    }

    private bool BitterSlip(EntityUid target)
    {
        bool got_smitted;
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
        return got_smitted;
    }

    private bool BitterSlow(EntityUid target)
    {
        float slowDown = 0.95f;
        var movementSpeed = EnsureComp<MovementSpeedModifierComponent>(target);
        (movementSpeed.BaseSprintSpeed, movementSpeed.BaseWalkSpeed) = (movementSpeed.BaseSprintSpeed * slowDown, movementSpeed.BaseWalkSpeed * slowDown);

        Dirty(target, movementSpeed);

        _popupSystem.PopupEntity("You feel a bit slower", target,
            target, PopupType.LargeCaution);
        return true;
    }

    private bool BitterSpeakBackwards(EntityUid target)
    {
        EnsureComp<BackwardsAccentComponent>(target);
        return true;
    }
    #endregion
    #region Harsh
    // Quite annoying things that can be immpossible to fix
    private void AnyHarshSmite(int i, EntityUid target, ref bool got_smitted)
    {
        i = _random.Next(7);
        switch (i)
        {
            case 0:
                EnsureComp<CluwneComponent>(target);
                got_smitted = true;
                break;
            case 1:
                got_smitted = HarshByeHand(target);
                break;
            case 2:
                got_smitted = HarshLockInLocker(target);
                break;
            case 3:
                got_smitted = HarshMaid(target);
                break;
            case 4:
                got_smitted = HarshMessySpeach(target);
                break;
            case 5:
                got_smitted = HarshSlow(target);
                break;
            case 6:
                got_smitted = HarshSwapRunAndWalk(target);
                break;
            default:
                break;
        }
    }


    private bool HarshMessySpeach(EntityUid target)
    {
        bool got_smitted;
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
        return got_smitted;
    }

    private bool HarshByeHand(EntityUid target)
    {
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
            return true;
        }
        return false;
    }

    private bool HarshMaid(EntityUid target)
    {
        SetOutfitCommand.SetOutfit(target, "JanitorMaidGear", EntityManager, (_, clothing) =>
        {
            if (HasComp<ClothingComponent>(clothing))
                EnsureComp<UnremoveableComponent>(clothing);
            EnsureComp<ClumsyComponent>(target);
        });
        return true;
    }

    private bool HarshSwapRunAndWalk(EntityUid target)
    {
        var movementSpeed = EnsureComp<MovementSpeedModifierComponent>(target);
        (movementSpeed.BaseSprintSpeed, movementSpeed.BaseWalkSpeed) = (movementSpeed.BaseWalkSpeed, movementSpeed.BaseSprintSpeed);

        Dirty(target, movementSpeed);

        _popupSystem.PopupEntity(Loc.GetString("admin-smite-run-walk-swap-prompt"), target,
            target, PopupType.LargeCaution);
        return true;
    }

    private bool HarshLockInLocker(EntityUid target)
    {
        bool got_smitted;
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
        return got_smitted;
    }

    private bool HarshSlow(EntityUid target)
    {
        float slowDown = 0.6f;
        var movementSpeed = EnsureComp<MovementSpeedModifierComponent>(target);
        (movementSpeed.BaseSprintSpeed, movementSpeed.BaseWalkSpeed) = (movementSpeed.BaseSprintSpeed * slowDown, movementSpeed.BaseWalkSpeed * slowDown);

        Dirty(target, movementSpeed);

        _popupSystem.PopupEntity("You feel a quite a bit slower", target,
            target, PopupType.LargeCaution);
        return true;
    }
    #endregion
    #region Nasty
    // Fate worse then or about as bad as death
    private void AnyNastySmite(int i, EntityUid target, ref bool got_smitted)
    {
        i = _random.Next(7);
        switch (i)
        {
            case 0:
                _polymorphSystem.PolymorphEntity(target, "AdminMonkeySmite");
                got_smitted = true;
                break;
            case 1:
                _polymorphSystem.PolymorphEntity(target, "AdminDisposalsSmite");
                got_smitted = true;
                break;
            case 2:
                _polymorphSystem.PolymorphEntity(target, "AdminBreadSmite");
                got_smitted = true;
                break;
            case 3:
                _polymorphSystem.PolymorphEntity(target, "AdminMouseSmite");
                got_smitted = true;
                break;
            case 4:
                got_smitted = NastyByeHands(target);
                break;
            case 5:
                got_smitted = NastyByeStomach(target);
                break;
            case 6:
                got_smitted = NastyPinball(target);
                break;
            case 7:
                break;
            case 8:
                break;
            case 9:
                break;
            default:
                break;
        }
    }

    private bool NastyByeHands(EntityUid target)
    {
        bool got_smitted;
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
        return got_smitted;
    }

    private bool NastyByeStomach(EntityUid target)
    {
        if (TryComp<BodyComponent>(target, out var body2))
        {
            foreach (var entity in _bodySystem.GetBodyOrganEntityComps<StomachComponent>((target, body2)))
            {
                QueueDel(entity.Owner);
            }

            _popupSystem.PopupEntity(Loc.GetString("admin-smite-stomach-removal-self"), target,
                target, PopupType.LargeCaution);
            return true;
        }

        return false;
    }

    private bool NastyPinball(EntityUid target) // Caused server error on restart
    {
        PhysicsComponent? physics;
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
            return true;
        }
        return false;
    }
    #endregion
    #region Harm
    // Stuff that damages the player without a kill
    private void AnyHarmSmite(int i, EntityUid target, ref bool got_smitted)
    {
        i = _random.Next(4);
        switch (i)
        {
            case 0:
                got_smitted = HarmBleeding(target);
                break;
            case 1:
                got_smitted = HarmBurn(target);
                break;
            case 2:
                got_smitted = HarmElectricute(target);
                break;
            case 3:
                got_smitted = HarmBleeding(target);
                break;
            default:
                break;
        }
    }

    private bool HarmBurn(EntityUid target)
    {
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
            return true;
        }
        return false;
    }

    private bool HarmBleeding(EntityUid target)
    {
        if (TryComp<BloodstreamComponent>(target, out var bloodstream))
        {
            _bloodstreamSystem.TryModifyBleedAmount(target, 8);
            var xform4 = Transform(target);
            _popupSystem.PopupEntity(Loc.GetString("You feel a sharpness in the air, blood rushes out!"), target,
                target, PopupType.MediumCaution);
            return true;
        }
        return false;
    }

    private bool HarmElectricute(EntityUid target)
    {
        if (TryComp<DamageableComponent>(target, out var damageable) &&
                            HasComp<MobStateComponent>(target))
        {
            int damageToDeal = 35;

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
            return true;
        }
        return false;
    }
    #endregion
    #region Kill
    /// Kill should for the most part kill the person and possibly round remove them.
    private void AnyKillSmite(int i, EntityUid target, ref bool got_smitted)
    {
        i = _random.Next(10);
        switch (i)
        {
            case 0:
                got_smitted = KillAsh(target);
                break;
            case 1:
                got_smitted = KillByeBlood(target);
                break;
            case 2:
                got_smitted = KillByeLungs(target);
                break;
            case 3:
                got_smitted = KillByeOrgans(target);
                break;
            case 4:
                got_smitted = KillElectricute(target);
                break;
            case 5:
                got_smitted = KillGibBoom(target);
                break;
            case 6:
                got_smitted = KillTooFast(target);
                break;
            case 7:
                got_smitted = KillYeet(target);
                break;
            case 8:
                EnsureComp<KillSignComponent>(target);
                got_smitted = true;
                break;
            case 9:
                EnsureComp<PointingArrowAngeringComponent>(target);
                got_smitted = true;
                break;
            default:
                break;
        }
    }

    private bool KillByeBlood(EntityUid target)
    {
        if (TryComp<BloodstreamComponent>(target, out var bloodstream))
        {
            _bloodstreamSystem.SpillAllSolutions(target, bloodstream);
            var xform4 = Transform(target);
            _popupSystem.PopupEntity(Loc.GetString("admin-smite-remove-blood-self"), target,
                target, PopupType.LargeCaution);
            _popupSystem.PopupCoordinates(Loc.GetString("admin-smite-remove-blood-others", ("name", target)), xform4.Coordinates,
                Filter.PvsExcept(target), true, PopupType.MediumCaution);
            return true;
        }
        return false;
    }

    private bool KillElectricute(EntityUid target)
    {
        if (TryComp<DamageableComponent>(target, out var damageable) &&
                            HasComp<MobStateComponent>(target))
        {
            int damageToDeal;
            if (!_mobThresholdSystem.TryGetThresholdForState(target, MobState.Critical, out var criticalThreshold))
            {
                // We can't crit them so try killing them.
                if (!_mobThresholdSystem.TryGetThresholdForState(target, MobState.Dead,
                        out var deadThreshold))
                    return false;// whelp.
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
            return true;
        }
        return false;
    }

    private bool KillByeOrgans(EntityUid target)
    {
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
            return true;
        }
        return false;
    }

    private bool KillGibBoom(EntityUid target)
    {
        bool got_smitted;
        var coords = _transformSystem.GetMapCoordinates(target);
        Timer.Spawn(_gameTiming.TickPeriod,
            () => _explosionSystem.QueueExplosion(coords, ExplosionSystem.DefaultExplosionPrototypeId,
                4, 1, 2, target, maxTileBreak: 0), // it gibs, damage doesn't need to be high.
            CancellationToken.None);

        _bodySystem.GibBody(target);
        got_smitted = true;
        return got_smitted;
    }

    private bool KillYeet(EntityUid target)
    {
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
            return true;
        }
        return false;
    }

    private bool KillByeLungs(EntityUid target) // Not guranteed kill, super annoying if it doesn't kill you
    {
        if (TryComp<BodyComponent>(target, out var body3))
        {
            foreach (var entity in _bodySystem.GetBodyOrganEntityComps<LungComponent>((target, body3)))
            {
                QueueDel(entity.Owner);
            }

            _popupSystem.PopupEntity(Loc.GetString("admin-smite-lung-removal-self"), target,
                target, PopupType.LargeCaution);
            return true;
        }
        return false;
    }

    private bool KillAsh(EntityUid target)
    {
        bool got_smitted;
        EntityManager.QueueDeleteEntity(target);
        Spawn("Ash", Transform(target).Coordinates);
        _popupSystem.PopupEntity(Loc.GetString("admin-smite-turned-ash-other", ("name", target)), target, PopupType.LargeCaution);
        got_smitted = true;
        return got_smitted;
    }

    private bool KillTooFast(EntityUid target)
    {
        var movementSpeed = EnsureComp<MovementSpeedModifierComponent>(target);
        _movementSpeedModifierSystem?.ChangeBaseSpeed(target, 400, 8000, 40, movementSpeed);

        _popupSystem.PopupEntity(Loc.GetString("admin-smite-super-speed-prompt"), target,
            target, PopupType.LargeCaution);
        return true;
    }
    #endregion
    #region Removed

    // private bool HarshWeightlessness(EntityUid target) // Doesn't work makes them jitter a bunch not fun
    // {
    //     bool got_smitted;
    //     var grav = EnsureComp<MovementIgnoreGravityComponent>(target);
    //     grav.Weightless = true;

    //     Dirty(target, grav);
    //     got_smitted = true;
    //     return got_smitted;
    // }



    // private bool BitterCatEars(EntityUid target, bool got_smitted) // Some people might like this
    // {
    //     if (TryComp<InventoryComponent>(target, out var inventory))
    //     {
    //         var ears = Spawn("ClothingHeadHatCatEars", Transform(target).Coordinates);
    //         EnsureComp<UnremoveableComponent>(ears);
    //         _inventorySystem.TryUnequip(target, "head", true, true, false, inventory);
    //         _inventorySystem.TryEquip(target, ears, "head", true, true, false, inventory);
    //         got_smitted = true;
    //     }

    //     return got_smitted;
    // }
    #endregion
}
