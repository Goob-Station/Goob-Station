using System.Linq;
using System.Numerics;
using Content.Server.Bible.Components;
using Content.Server.Chat.Systems;
using Content.Server.Chemistry.Components;
using Content.Server.DoAfter;
using Content.Server.Fluids.Components;
using Content.Server.Popups;
using Content.Server.WhiteDream.BloodCult.Empower;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Maps;
using Content.Shared.WhiteDream.BloodCult.BloodCultist;
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
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        // Drawing rune
        SubscribeLocalEvent<RuneDrawerComponent, RuneDrawerSelectedMessage>(OnRuneSelected);
        SubscribeLocalEvent<BloodCultistComponent, DrawRuneDoAfter>(OnDrawRune);

        // Erasing rune
        SubscribeLocalEvent<CultRuneBaseComponent, InteractUsingEvent>(EraseOnInteractUsing);
        SubscribeLocalEvent<CultRuneBaseComponent, RuneEraseDoAfterEvent>(OnRuneErase);
        SubscribeLocalEvent<CultRuneBaseComponent, StartCollideEvent>(EraseOnCollding);

        // Rune invoking
        SubscribeLocalEvent<CultRuneBaseComponent, ActivateInWorldEvent>(OnRuneActivate);
    }

    private void OnRuneSelected(Entity<RuneDrawerComponent> ent, ref RuneDrawerSelectedMessage args)
    {
        if (!_protoManager.TryIndex(args.SelectedRune, out var runeSelector) || !CanDrawRune(args.Actor))
            return;

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
        var rune = SpawnRune(args.User, runeSelector.Prototype);

        var ev = new AfterRunePlaced(args.User);
        RaiseLocalEvent(rune, ev);
    }

    private void EraseOnInteractUsing(Entity<CultRuneBaseComponent> ent, ref InteractUsingEvent args)
    {
        // Logic for bible erasing
        if (TryComp<BibleComponent>(args.Used, out var bible) && HasComp<BibleUserComponent>(args.User))
        {
            _popup.PopupEntity(Loc.GetString("cult-rune-erased"), ent, args.User);
            _audio.PlayPvs(bible.HealSoundPath, args.User);
            EntityManager.DeleteEntity(args.Target);
            return;
        }

        if (!TryComp(args.Used, out RuneDrawerComponent? runeDrawer))
            return;

        var argsDoAfterEvent =
            new DoAfterArgs(EntityManager, args.User, runeDrawer.EraseTime, new RuneEraseDoAfterEvent(), ent)
            {
                BreakOnUserMove = true,
                BreakOnDamage = true,
                NeedHand = true
            };

        if (_doAfter.TryStartDoAfter(argsDoAfterEvent))
            _popup.PopupEntity(Loc.GetString("cult-rune-started-erasing"), ent, args.User);
    }

    private void OnRuneErase(Entity<CultRuneBaseComponent> ent, ref RuneEraseDoAfterEvent args)
    {
        if (args.Cancelled)
            return;

        _popup.PopupEntity(Loc.GetString("cult-rune-erased"), ent, args.User);
        EntityManager.DeleteEntity(ent);
    }

    private void EraseOnCollding(Entity<CultRuneBaseComponent> ent, ref StartCollideEvent args)
    {
        if (!TryComp<SolutionContainerManagerComponent>(args.OtherEntity, out var solutionContainer) ||
            !HasComp<VaporComponent>(args.OtherEntity) && !HasComp<SprayComponent>(args.OtherEntity))
            return;

        if (_solutionContainer.EnumerateSolutions((args.OtherEntity, solutionContainer))
            .Any(solution => solution.Solution.Comp.Solution.ContainsPrototype(ent.Comp.HolyWaterPrototype)))
            EntityManager.DeleteEntity(ent);
    }

    private void OnRuneActivate(Entity<CultRuneBaseComponent> ent, ref ActivateInWorldEvent args)
    {
        var runeCoordinates = Transform(ent).Coordinates;
        var userCoordinates = Transform(args.User).Coordinates;
        if (args.Handled || !HasComp<BloodCultistComponent>(args.User) ||
            !userCoordinates.TryDistance(EntityManager, runeCoordinates, out var distance) ||
            distance > ent.Comp.RuneActivationRange)
            return;

        args.Handled = true;

        var cultists = GatherCultists(ent, ent.Comp.RuneActivationRange);
        if (cultists.Count < ent.Comp.RequiredInvokers)
        {
            _popup.PopupEntity(Loc.GetString("cult-rune-not-enough-cultists"), ent, args.User);
            return;
        }

        var tryInvokeEv = new TryInvokeCultRuneEvent(args.User, cultists);
        RaiseLocalEvent(ent, tryInvokeEv);
        if (tryInvokeEv.Cancelled)
            return;

        foreach (var cultist in cultists)
        {
            DealDamage(cultist, ent.Comp.ActivationDamage);
            _chat.TrySendInGameICMessage(cultist,
                ent.Comp.InvokePhrase,
                ent.Comp.InvokeChatType,
                false,
                checkRadioPrefix: false);
        }
    }

    private EntityUid SpawnRune(EntityUid user, EntProtoId rune)
    {
        var transform = Transform(user);
        var snappedLocalPosition = new Vector2(MathF.Floor(transform.LocalPosition.X) + 0.5f,
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
            {
                damage.DamageDict[key] = value * empowered.RuneDamageMultiplier;
            }
        }

        _damageable.TryChangeDamage(user, newDamage, true);
    }
}
