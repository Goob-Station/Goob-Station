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
    // private readonly Random _rnd = new Random();
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
        var i = 2;
        i /= 2;
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
    //             if (TryComp<FlammableComponent>(args.Target, out var flammable))
    //             {
    //                 // Fuck you. Burn Forever.
    //                 flammable.FireStacks = flammable.MaximumFireStacks;
    //                 _flammableSystem.Ignite(args.Target, args.User);
    //                 var xform = Transform(args.Target);
    //                 _popupSystem.PopupEntity(Loc.GetString("admin-smite-set-alight-self"), args.Target,
    //                     args.Target, PopupType.LargeCaution);
    //                 _popupSystem.PopupCoordinates(Loc.GetString("admin-smite-set-alight-others", ("name", args.Target)), xform.Coordinates,
    //                     Filter.PvsExcept(args.Target), true, PopupType.MediumCaution);
    //             }
    //             break;
    //         case 7:
    //             if (TryComp<DamageableComponent>(args.Target, out var damageable) &&
    //                 HasComp<MobStateComponent>(args.Target))
    //             {
    //                 int damageToDeal;
    //                 if (!_mobThresholdSystem.TryGetThresholdForState(args.Target, MobState.Critical, out var criticalThreshold)) {
    //                     // We can't crit them so try killing them.
    //                     if (!_mobThresholdSystem.TryGetThresholdForState(args.Target, MobState.Dead,
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

    //                 if (_inventorySystem.TryGetSlots(args.Target, out var slotDefinitions))
    //                 {
    //                     foreach (var slot in slotDefinitions)
    //                     {
    //                         if (!_inventorySystem.TryGetSlotEntity(args.Target, slot.Name, out var slotEnt))
    //                             continue;

    //                         RemComp<InsulatedComponent>(slotEnt.Value); // Fry the gloves.
    //                     }
    //                 }

    //                 _electrocutionSystem.TryDoElectrocution(args.Target, null, damageToDeal,
    //                     TimeSpan.FromSeconds(30), refresh: true, ignoreInsulation: true);
    //             }
    //             break;
    //         case 8:
    //             if (TryComp<CreamPiedComponent>(args.Target, out var creamPied))
    //             {
    //                 Text = creamPieName,
    //                 Category = VerbCategory.Smite,
    //                 Icon = new SpriteSpecifier.Rsi(new ("/Textures/Objects/Consumable/Food/Baked/pie.rsi"), "plain-slice"),
    //                 Act = () =>
    //                 {
    //                     _creamPieSystem.SetCreamPied(args.Target, creamPied, true);
    //                 },
    //                 Impact = LogImpact.Extreme,
    //                 Message = string.Join(": ", creamPieName, Loc.GetString("admin-smite-creampie-description"))
    //             }
    //             break;
    //         case 9:
    //             break;
    //     }
}
