using System.Linq;
using Content.Shared.Atmos;
using Content.Shared.Charges.Systems;
using Content.Shared.Eye;
using Content.Shared.Hands.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Revenant.Components;
using Content.Shared.StatusEffectNew;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Physics.Events;

namespace Content.Goobstation.Shared.Wraith.SpiritCandle;

/// <summary>
/// This handles spirit candles.
/// Once lit up, they reveal evil spirits in a 12x12 tile area.
/// When used in hand, it makes the corporeal and weakened for some seconds.
/// </summary>
public sealed partial class SharedSpiritCandleSystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly SharedVisibilitySystem _visibility = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly Content.Shared.StatusEffect.StatusEffectsSystem _oldStatusEffects = default!;
    [Dependency] private readonly SharedChargesSystem _charges = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpiritCandleComponent, IgnitedEvent>(OnIgnited);
        SubscribeLocalEvent<SpiritCandleComponent, ExtinguishedEvent>(OnExtinguished);

        SubscribeLocalEvent<SpiritCandleComponent, EntGotInsertedIntoContainerMessage>(OnEntInserted);
        SubscribeLocalEvent<SpiritCandleComponent, EntGotRemovedFromContainerMessage>(OnEntRemoved);

        SubscribeLocalEvent<SpiritCandleComponent, UseInHandEvent>(OnUseInHand);

        SubscribeLocalEvent<SpiritCandleAreaComponent, StartCollideEvent>(OnStartCollide);
        SubscribeLocalEvent<SpiritCandleAreaComponent, EndCollideEvent>(OnEndCollide);

        SubscribeLocalEvent<CorporealComponent, AttemptCollideSpiritCandleEvent>(OnAttemptCollideSpiritCandle);
    }

    #region Spirit Candle

    private void OnIgnited(Entity<SpiritCandleComponent> ent, ref IgnitedEvent args)
    {
        if (_netManager.IsClient)
            return;

        ent.Comp.Active = true;

        if (ent.Comp.Holder is { } holder)
        {
            var spawn = SpawnAttachedTo(ent.Comp.SpiritArea, Transform(holder).Coordinates);
            _transform.SetParent(spawn, holder);
            ent.Comp.AreaUid = spawn;
        }
        else
        {
            ent.Comp.Holder = null;
            var spawn = SpawnAttachedTo(ent.Comp.SpiritArea, Transform(ent.Owner).Coordinates);
            _transform.SetParent(spawn, ent.Owner);
            ent.Comp.AreaUid = spawn;
        }
    }

    private void OnExtinguished(Entity<SpiritCandleComponent> ent, ref ExtinguishedEvent args)
    {
        if (_netManager.IsClient)
            return;

        ent.Comp.Active = false;

        if (ent.Comp.AreaUid is not {} areaUid)
            return;

        QueueDel(areaUid);
        ent.Comp.AreaUid = null;
    }

    private void OnEntInserted(Entity<SpiritCandleComponent> ent, ref EntGotInsertedIntoContainerMessage args)
    {
        if (_netManager.IsClient || !ent.Comp.Active)
            return;

        if (ent.Comp.AreaUid is {} areaUid)
            QueueDel(areaUid);

        if (!HasComp<HandsComponent>(args.Container.Owner))
        {
            ent.Comp.AreaUid = null;
            ent.Comp.Holder = null;
            return;
        }

        var spawn = SpawnAttachedTo(ent.Comp.SpiritArea, Transform(args.Container.Owner).Coordinates);
        _transform.SetParent(spawn, args.Container.Owner);
        ent.Comp.AreaUid = spawn;
        ent.Comp.Holder = args.Container.Owner;
    }

    private void OnEntRemoved(Entity<SpiritCandleComponent> ent, ref EntGotRemovedFromContainerMessage args)
    {
        if (_netManager.IsClient || !ent.Comp.Active)
            return;

        if (ent.Comp.AreaUid is {} areaUid)
            QueueDel(areaUid);

        var spawn = SpawnAttachedTo(ent.Comp.SpiritArea, Transform(args.Entity).Coordinates);
        _transform.SetParent(spawn, args.Entity);
        ent.Comp.AreaUid = spawn;
        ent.Comp.Holder = null;
    }

    private void OnUseInHand(Entity<SpiritCandleComponent> ent, ref UseInHandEvent args)
    {
        if (ent.Comp.AreaUid is not {} areaUid || !TryComp<SpiritCandleAreaComponent>(areaUid, out var area))
            return;

        if (!area.EntitiesInside.Any())
        {
            _popup.PopupEntity(Loc.GetString("spirit-candle-fail"), args.User, args.User, PopupType.MediumCaution);
            return;
        }

        if (_charges.IsEmpty(ent.Owner))
            return;

        foreach (var ghost in area.EntitiesInside)
        {
            if (ghost is not {} ghostUid)
                continue;

            _oldStatusEffects.TryAddStatusEffect<CorporealComponent>(ghostUid, ent.Comp.Corporeal, ent.Comp.CorporealDuration, true);
            _statusEffects.TryAddStatusEffectDuration(ghostUid, ent.Comp.Weakened, out _, ent.Comp.WeakenedDuration);
        }

        _popup.PopupEntity(Loc.GetString("spirit-candle-caught-wraith"), args.User, args.User, PopupType.LargeCaution);
        _audio.PlayPvs(ent.Comp.SuccessSound, args.User);

        _charges.TryUseCharge(ent.Owner);
        _appearance.SetData(ent.Owner, SpiritCandleVisuals.Layer, _charges.GetCurrentCharges(ent.Owner));
    }

    #endregion

    #region Area
    private void OnStartCollide(Entity<SpiritCandleAreaComponent> ent, ref StartCollideEvent args)
    {
        var ev = new AttemptCollideSpiritCandleEvent();
        RaiseLocalEvent(args.OtherEntity, ref ev);

        if (ev.Cancelled)
            return;

        if (!_whitelist.IsWhitelistPass(ent.Comp.Whitelist, args.OtherEntity))
            return;

        if (!TryComp<VisibilityComponent>(args.OtherEntity, out var visibility))
            return;

        var otherEnt = (args.OtherEntity, visibility);

        ent.Comp.EntitiesInside.Add(args.OtherEntity);
        Dirty(ent);

        _visibility.RemoveLayer(otherEnt, (int) VisibilityFlags.Ghost, false);
        _visibility.SetLayer(otherEnt, (int) VisibilityFlags.Normal, false);
        _visibility.RefreshVisibility(otherEnt);
    }

    private void OnEndCollide(Entity<SpiritCandleAreaComponent> ent, ref EndCollideEvent args)
    {
        var ev = new AttemptCollideSpiritCandleEvent();
        RaiseLocalEvent(args.OtherEntity, ref ev);

        if (ev.Cancelled)
            return;

        if (!_whitelist.IsWhitelistPass(ent.Comp.Whitelist, args.OtherEntity))
            return;

        if (!TryComp<VisibilityComponent>(args.OtherEntity, out var visibility))
            return;

        var otherEnt = (args.OtherEntity, visibility);

        ent.Comp.EntitiesInside.Remove(args.OtherEntity);
        Dirty(ent);

        _visibility.AddLayer(otherEnt, (int) VisibilityFlags.Ghost, false);
        _visibility.RemoveLayer(otherEnt, (int) VisibilityFlags.Normal, false);
        _visibility.RefreshVisibility(otherEnt);
    }

    private void OnAttemptCollideSpiritCandle(Entity<CorporealComponent> ent, ref AttemptCollideSpiritCandleEvent args) =>
        args.Cancelled = true; // if already corporeal, don't do anything

    #endregion
}

