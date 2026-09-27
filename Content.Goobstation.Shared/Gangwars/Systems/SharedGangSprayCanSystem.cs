using Content.Goobstation.Shared.Gangwars.Components;
using Content.Goobstation.Shared.Gangwars.Events;
using Content.Shared.Charges.Systems;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Random;
using Content.Shared.Random.Helpers;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Gangwars.Systems;

public sealed class SharedGangSprayCanSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly GangwarRuleSystem _gangwarRule = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedChargesSystem _charges = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GangSprayCanComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<GangSprayCanComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<GangSprayCanComponent, GangSprayCanDoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<GangClothingComponent, InteractUsingEvent>(OnClothingInteractUsing);
    }

    private void OnExamined(Entity<GangSprayCanComponent> ent, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        using var _ = args.PushGroup(nameof(GangSprayCanComponent));
        args.PushMarkup(Loc.GetString("gang-spray-can-charges-remaining", ("charges", _charges.GetCurrentCharges(ent.Owner))));
    }

    private void OnAfterInteract(Entity<GangSprayCanComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach)
            return;

        TrySpray(ent, args.User, args.ClickLocation, args.Target);
    }

    private void OnClothingInteractUsing(Entity<GangClothingComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<GangSprayCanComponent>(args.Used, out var sprayCan))
            return;

        TrySpray(new Entity<GangSprayCanComponent>(args.Used, sprayCan), args.User, _transform.GetMoverCoordinates(ent), ent);
        args.Handled = true;
    }

    private void TrySpray(Entity<GangSprayCanComponent> ent, EntityUid user, EntityCoordinates target, EntityUid? interactTarget)
    {
        if (!target.IsValid(EntityManager))
            return;

        if (!TryComp<GangMemberComponent>(user, out var gangMember) || gangMember.Gang == null)
        {
            _popup.PopupClient(Loc.GetString("gang-spray-can-no-gang"), user);
            return;
        }

        if (_charges.IsEmpty(ent.Owner))
        {
            _audio.PlayPredicted(ent.Comp.EmptySound, ent.Owner, user);
            _appearance.SetData(ent, GangSprayCanVisuals.Empty, true);
            return;
        }

        // If the target entity has GangClothingComponent, paint it instead of placing a territory sign.
        if (interactTarget != null && TryComp<GangClothingComponent>(interactTarget, out var clothing))
        {
            clothing.Gang = gangMember.Gang;
            Dirty(interactTarget.Value, clothing);

            var gangColor = EnsureComp<GangColorComponent>(interactTarget.Value);
            gangColor.GangColor = gangMember.Gang.Value;
            Dirty(interactTarget.Value, gangColor);

            var clothingColorEv = new GangColorAppliedEvent();
            RaiseLocalEvent(interactTarget.Value, ref clothingColorEv);

            _charges.TryUseCharge(ent.Owner);

            if (_charges.IsEmpty(ent.Owner))
                _appearance.SetData(ent, GangSprayCanVisuals.Empty, true);

            _audio.PlayPredicted(ent.Comp.UseSound, ent.Owner, user);
            _popup.PopupClient(Loc.GetString("gang-spray-can-clothing-painted"), user);
            return;
        }

        var snappedTarget = target.SnapToGrid(EntityManager);
        var newSignRadius = new GangTerritoryComponent().TerritoryRadius;
        if (_gangwarRule.IsTerritoryTooClose(_transform.ToMapCoordinates(snappedTarget), newSignRadius, gangMember.Gang))
        {
            _popup.PopupClient(Loc.GetString("gang-territory-too-close"), user);
            return;
        }

        var doAfterArgs = new DoAfterArgs(EntityManager,
            user,
            ent.Comp.SprayDelay,
            new GangSprayCanDoAfterEvent { ClickLocation = GetNetCoordinates(snappedTarget), Gang = gangMember.Gang.Value },
            ent,
            target: null,
            used: ent)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = true,
            BlockDuplicate = true,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnDoAfter(Entity<GangSprayCanComponent> ent, ref GangSprayCanDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        args.Handled = true;

        var snappedTarget = GetCoordinates(args.ClickLocation).SnapToGrid(EntityManager);
        PlaceSign(ent.Comp.SignPrototypes, snappedTarget, args.Gang, ent.Owner);

        _charges.TryUseCharge(ent.Owner);
        _audio.PlayPredicted(ent.Comp.UseSound, ent.Owner, args.User);

        if (TryComp<GangMemberComponent>(args.User, out var gangMember))
        {
            gangMember.GangPoints += ent.Comp.GangPoints;
            Dirty(args.User, gangMember);

            _popup.PopupClient(Loc.GetString("gang-spray-can-points-earned", ("points", ent.Comp.GangPoints)), args.User);
        }
    }

    /// <summary>
    /// Prediction is annoying
    /// </summary>
    private void PlaceSign(ProtoId<WeightedRandomPrototype> signPrototypes, EntityCoordinates location, Color color, EntityUid sprayCan)
    {
        var signs = _proto.Index(signPrototypes);
        var seed = SharedRandomExtensions.HashCodeCombine((int) _timing.CurTick.Value, GetNetEntity(sprayCan).Id);
        var signProto = signs.Pick(new Random(seed));
        var uid = PredictedSpawnAtPosition(signProto, location);

        var gangColor = EnsureComp<GangColorComponent>(uid);
        gangColor.GangColor = color;
        Dirty(uid, gangColor);

        var territory = EnsureComp<GangTerritoryComponent>(uid);
        territory.GangColor = color;
        Dirty(uid, territory);

        // This stops it from showing up as the default color for a second on spawn
        var colorEv = new GangColorAppliedEvent();
        RaiseLocalEvent(uid, ref colorEv);
    }
}
