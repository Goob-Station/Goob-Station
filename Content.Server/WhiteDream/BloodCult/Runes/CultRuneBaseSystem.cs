using System.Linq;
using System.Numerics;
using Content.Server.Bible.Components;
using Content.Server.Chat.Systems;
using Content.Server.Chemistry.Components;
using Content.Server.DoAfter;
using Content.Server.Fluids.Components;
using Content.Server.Popups;
using Content.Server.WhiteDream.BloodCult.Empower;
using Content.Server.WhiteDream.BloodCult.Gamerule;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Ghost;
using Content.Shared.Interaction;
using Content.Shared.Maps;
using Content.Shared.UserInterface;
using Content.Shared.WhiteDream.BloodCult.BloodCultist;
using Content.Shared.WhiteDream.BloodCult.Constructs;
using Content.Shared.WhiteDream.BloodCult.Runes;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Physics.Events;
using Robust.Shared.Prototypes;

namespace Content.Server.WhiteDream.BloodCult.Runes;

public sealed partial class CultRuneBaseSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly BloodCultRuleSystem _cultRule = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        // Drawing rune
        SubscribeLocalEvent<RuneDrawerComponent, BeforeActivatableUIOpenEvent>(BeforeOpenUi);
        SubscribeLocalEvent<RuneDrawerComponent, RuneDrawerSelectedMessage>(OnRuneSelected);
        SubscribeLocalEvent<BloodCultistComponent, DrawRuneDoAfter>(OnDrawRune);

        // Erasing rune
        SubscribeLocalEvent<CultRuneBaseComponent, InteractUsingEvent>(EraseOnInteractUsing);
        SubscribeLocalEvent<CultRuneBaseComponent, RuneEraseDoAfterEvent>(OnRuneErase);
        SubscribeLocalEvent<CultRuneBaseComponent, StartCollideEvent>(EraseOnCollding);

        // Rune invoking
        SubscribeLocalEvent<CultRuneBaseComponent, ActivateInWorldEvent>(OnRuneActivate);

        SubscribeLocalEvent<CultRuneBaseComponent, ExamineAttemptEvent>(OnRuneExaminaAttempt);
    }

    #region EventHandlers

    private void BeforeOpenUi(Entity<RuneDrawerComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        var availableRunes = new List<ProtoId<RuneSelectorPrototype>>();
        var runeSelectorArray = _protoManager.EnumeratePrototypes<RuneSelectorPrototype>().OrderBy(r => r.ID).ToArray();
        foreach (var runeSelector in runeSelectorArray)
        {
            if (runeSelector.RequireTargetDead && !_cultRule.IsObjectiveFinished() ||
                runeSelector.RequiredTotalCultists > _cultRule.GetTotalCultists())
                continue;

            availableRunes.Add(runeSelector.ID);
        }

        _ui.SetUiState(ent.Owner, RuneDrawerBuiKey.Key, new RuneDrawerMenuState(availableRunes));
    }

    private void OnRuneSelected(Entity<RuneDrawerComponent> ent, ref RuneDrawerSelectedMessage args)
    {
        if (!_protoManager.TryIndex(args.SelectedRune, out var runeSelector) || !CanDrawRune(args.Actor))
            return;

        if (runeSelector.RequireTargetDead && !_cultRule.CanDrawRendingRune(args.Actor))
        {
            _popup.PopupEntity(Loc.GetString("cult-rune-cant-draw-rending"), args.Actor, args.Actor);
            return;
        }

        var timeToDraw = runeSelector.DrawTime;
        if (TryComp(args.Actor, out BloodCultEmpoweredComponent? empowered))
            timeToDraw *= empowered.RuneTimeMultiplier;

        var ev = new DrawRuneDoAfter
        {
            Rune = args.SelectedRune,
            EndDrawingSound = ent.Comp.EndDrawingSound
        };

        var argsDoAfterEvent = new DoAfterArgs(EntityManager, args.Actor, timeToDraw, ev, args.Actor)
        {
            BreakOnUserMove = true,
            NeedHand = true
        };

        if (_doAfter.TryStartDoAfter(argsDoAfterEvent))
            _audio.PlayPvs(ent.Comp.StartDrawingSound, args.Actor, AudioParams.Default.WithMaxDistance(2f));
    }

    private void OnDrawRune(Entity<BloodCultistComponent> ent, ref DrawRuneDoAfter args)
    {
        if (args.Cancelled || !_protoManager.TryIndex(args.Rune, out var runeSelector))
            return;

        DealDamage(args.User, runeSelector.DrawDamage);

        _audio.PlayPvs(args.EndDrawingSound, args.User, AudioParams.Default.WithMaxDistance(2f));
        var runeEnt = SpawnRune(args.User, runeSelector.Prototype);
        if (TryComp(runeEnt, out CultRuneBaseComponent? rune) 
            && rune.TriggerRendingMarkers
            && !_cultRule.TryConsumeNearestMarker(ent))
            return;

        var ev = new AfterRunePlaced(args.User);
        RaiseLocalEvent(runeEnt, ev);
    }

    private void EraseOnInteractUsing(Entity<CultRuneBaseComponent> rune, ref InteractUsingEvent args)
    {
        if (!rune.Comp.CanBeErased)
            return;

        // Logic for bible erasing
        if (TryComp<BibleComponent>(args.Used, out var bible) && HasComp<BibleUserComponent>(args.User))
        {
            _popup.PopupEntity(Loc.GetString("cult-rune-erased"), rune, args.User);
            _audio.PlayPvs(bible.HealSoundPath, args.User);
            EntityManager.DeleteEntity(args.Target);
            return;
        }

        if (!TryComp(args.Used, out RuneDrawerComponent? runeDrawer))
            return;

        var argsDoAfterEvent =
            new DoAfterArgs(EntityManager, args.User, runeDrawer.EraseTime, new RuneEraseDoAfterEvent(), rune)
            {
                BreakOnUserMove = true,
                BreakOnDamage = true,
                NeedHand = true
            };

        if (_doAfter.TryStartDoAfter(argsDoAfterEvent))
            _popup.PopupEntity(Loc.GetString("cult-rune-started-erasing"), rune, args.User);
    }

    private void OnRuneErase(Entity<CultRuneBaseComponent> ent, ref RuneEraseDoAfterEvent args)
    {
        if (args.Cancelled)
            return;

        _popup.PopupEntity(Loc.GetString("cult-rune-erased"), ent, args.User);
        EntityManager.DeleteEntity(ent);
    }

    private void EraseOnCollding(Entity<CultRuneBaseComponent> rune, ref StartCollideEvent args)
    {
        if (!rune.Comp.CanBeErased ||
            !TryComp<SolutionContainerManagerComponent>(args.OtherEntity, out var solutionContainer) ||
            !HasComp<VaporComponent>(args.OtherEntity) && !HasComp<SprayComponent>(args.OtherEntity))
            return;

        if (_solutionContainer.EnumerateSolutions((args.OtherEntity, solutionContainer))
            .Any(solution => solution.Solution.Comp.Solution.ContainsPrototype(rune.Comp.HolyWaterPrototype)))
            EntityManager.DeleteEntity(rune);
    }

    private void OnRuneActivate(Entity<CultRuneBaseComponent> rune, ref ActivateInWorldEvent args)
    {
        var runeCoordinates = Transform(rune).Coordinates;
        var userCoordinates = Transform(args.User).Coordinates;
        if (args.Handled || !HasComp<BloodCultistComponent>(args.User) ||
            !userCoordinates.TryDistance(EntityManager, runeCoordinates, out var distance) ||
            distance > rune.Comp.RuneActivationRange)
            return;

        args.Handled = true;

        var cultists = GatherCultists(rune, rune.Comp.RuneActivationRange);
        if (cultists.Count < rune.Comp.RequiredInvokers)
        {
            _popup.PopupEntity(Loc.GetString("cult-rune-not-enough-cultists"), rune, args.User);
            return;
        }

        var tryInvokeEv = new TryInvokeCultRuneEvent(args.User, cultists);
        RaiseLocalEvent(rune, tryInvokeEv);
        if (tryInvokeEv.Cancelled)
            return;

        foreach (var cultist in cultists)
        {
            DealDamage(cultist, rune.Comp.ActivationDamage);
            _chat.TrySendInGameICMessage(
                cultist,
                rune.Comp.InvokePhrase,
                rune.Comp.InvokeChatType,
                false,
                checkRadioPrefix: false);
        }
    }

    private void OnRuneExaminaAttempt(Entity<CultRuneBaseComponent> rune, ref ExamineAttemptEvent args)
    {
        if (!HasComp<BloodCultistComponent>(args.Examiner) && !HasComp<ConstructComponent>(args.Examiner) &&
            !HasComp<GhostComponent>(args.Examiner))
            args.Cancel();
    }

    #endregion

    private EntityUid SpawnRune(EntityUid user, EntProtoId rune)
    {
        var transform = Transform(user);
        var snappedLocalPosition = new Vector2(
            MathF.Floor(transform.LocalPosition.X) + 0.5f,
            MathF.Floor(transform.LocalPosition.Y) + 0.5f);
        var spawnPosition = _transform.GetMapCoordinates(user);
        var runeEntity = EntityManager.Spawn(rune, spawnPosition);
        _transform.SetLocalPosition(runeEntity, snappedLocalPosition);

        return runeEntity;
    }

    private bool CanDrawRune(EntityUid uid)
    {
        var transform = Transform(uid);
        var gridUid = transform.GridUid;
        if (!gridUid.HasValue)
        {
            _popup.PopupEntity(Loc.GetString("cult-rune-cant-draw"), uid, uid);
            return false;
        }

        var tile = transform.Coordinates.GetTileRef();
        if (tile.HasValue)
            return true;

        _popup.PopupEntity(Loc.GetString("cult-cant-draw-rune"), uid, uid);
        return false;
    }

    private void DealDamage(EntityUid user, DamageSpecifier? damage = null)
    {
        if (damage is null)
            return;

        // So the original DamageSpecifier will not be changed.
        var newDamage = new DamageSpecifier(damage);
        if (TryComp(user, out BloodCultEmpoweredComponent? empowered))
        {
            foreach (var (key, value) in damage.DamageDict)
                damage.DamageDict[key] = value * empowered.RuneDamageMultiplier;
        }

        _damageable.TryChangeDamage(user, newDamage, true);
    }
}
