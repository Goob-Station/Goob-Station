using System.Linq;
using Content.Shared.Atmos;
using Content.Shared.Charges.Systems;
using Content.Shared.Eye;
using Content.Shared.Hands;
using Content.Shared.Interaction.Events;
using Content.Shared.Revenant.Components;
using Content.Shared.StatusEffectNew;
using Content.Shared.Whitelist;
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

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpiritCandleComponent, IgnitedEvent>(OnIgnited);
        SubscribeLocalEvent<SpiritCandleComponent, ExtinguishedEvent>(OnExtinguished);

        SubscribeLocalEvent<SpiritCandleComponent, GotEquippedHandEvent>(OnGotEquipped);
        SubscribeLocalEvent<SpiritCandleComponent, DroppedEvent>(OnDropped);

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

        var spawn = SpawnAttachedTo(ent.Comp.SpiritArea, Transform(ent.Owner).Coordinates);

        _transform.SetParent(spawn, ent.Owner);
        ent.Comp.AreaUid = spawn;
    }

    private void OnExtinguished(Entity<SpiritCandleComponent> ent, ref ExtinguishedEvent args)
    {
        if (_netManager.IsClient)
            return;

        if (ent.Comp.AreaUid is not {} areaUid)
            return;

        QueueDel(areaUid);
        ent.Comp.AreaUid = null;
    }

    private void OnGotEquipped(Entity<SpiritCandleComponent> ent, ref GotEquippedHandEvent args)
    {
        if (_netManager.IsClient)
            return;

        if (ent.Comp.AreaUid is not {} areaUid)
            return;

        QueueDel(areaUid);
        var spawn = SpawnAttachedTo(ent.Comp.SpiritArea, Transform(args.User).Coordinates);
        _transform.SetParent(spawn, args.User);
        ent.Comp.AreaUid = spawn;
    }

    private void OnDropped(Entity<SpiritCandleComponent> ent, ref DroppedEvent args)
    {
        if (_netManager.IsClient)
            return;

        if (ent.Comp.AreaUid is not {} areaUid)
            return;

        QueueDel(areaUid);
        var spawn = SpawnAttachedTo(ent.Comp.SpiritArea, Transform(ent.Owner).Coordinates);
        _transform.SetParent(spawn, ent.Owner);
        ent.Comp.AreaUid = spawn;
    }

    private void OnUseInHand(Entity<SpiritCandleComponent> ent, ref UseInHandEvent args)
    {
        if (ent.Comp.AreaUid is not {} areaUid || !TryComp<SpiritCandleAreaComponent>(areaUid, out var area))
            return;

        if (!area.EntitiesInside.Any())
            return; // dont use charge if no entities inside  -popup here

        if (_charges.IsEmpty(ent.Owner))
            return; // popup here

        foreach (var ghost in area.EntitiesInside)
        {
            if (ghost is not {} ghostUid)
                continue;

            _oldStatusEffects.TryAddStatusEffect<CorporealComponent>(ghostUid, ent.Comp.Corporeal, ent.Comp.CorporealDuration, true);
            _statusEffects.TryAddStatusEffectDuration(ghostUid, ent.Comp.Weakened, out _, ent.Comp.WeakenedDuration);
        }

        _charges.TryUseCharge(ent.Owner);
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

