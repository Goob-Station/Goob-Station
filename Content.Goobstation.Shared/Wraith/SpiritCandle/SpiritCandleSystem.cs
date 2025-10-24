using System.Linq;
using Content.Shared.Atmos;
using Content.Shared.Charges.Systems;
using Content.Shared.Eye;
using Content.Shared.Interaction.Events;
using Content.Shared.Revenant.Components;
using Content.Shared.StatusEffectNew;
using Content.Shared.Whitelist;
using Robust.Shared.Physics.Events;

namespace Content.Goobstation.Shared.Wraith.SpiritCandle;

/// <summary>
/// This handles spirit candles.
/// Once lit up, they reveal evil spirits in a 12x12 tile area.
/// When used in hand, it makes the corporeal and weakened for some seconds.
/// </summary>
public sealed class SpiritCandleSystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly SharedVisibilitySystem _visibility = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly Content.Shared.StatusEffect.StatusEffectsSystem _oldStatusEffects = default!;
    [Dependency] private readonly SharedChargesSystem _charges = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpiritCandleComponent, IgnitedEvent>(OnIgnited);
        SubscribeLocalEvent<SpiritCandleComponent, ExtinguishedEvent>(OnExtinguished);

        SubscribeLocalEvent<SpiritCandleComponent, UseInHandEvent>(OnUseInHand);

        SubscribeLocalEvent<SpiritCandleAreaComponent, StartCollideEvent>(OnStartCollide);
        SubscribeLocalEvent<SpiritCandleAreaComponent, EndCollideEvent>(OnEndCollide);

        SubscribeLocalEvent<CorporealComponent, AttemptCollideSpiritCandleEvent>(OnAttemptCollideSpiritCandle);
    }

    #region Spirit Candle

    private void OnIgnited(Entity<SpiritCandleComponent> ent, ref IgnitedEvent args) =>
        ent.Comp.AreaUid = SpawnAttachedTo(ent.Comp.SpritiCandleArea, Transform(ent.Owner).Coordinates);

    private void OnExtinguished(Entity<SpiritCandleComponent> ent, ref ExtinguishedEvent args) =>
        ent.Comp.AreaUid = null;

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

        Log.Info("Activated spirit candle effects");

        ent.Comp.EntitiesInside.Add(args.OtherEntity);

        _visibility.RemoveLayer(args.OtherEntity, (int) VisibilityFlags.Ghost, false);
        _visibility.SetLayer(args.OtherEntity, (int) VisibilityFlags.Normal, false);
        _visibility.RefreshVisibility(args.OtherEntity);
    }

    private void OnEndCollide(Entity<SpiritCandleAreaComponent> ent, ref EndCollideEvent args)
    {
        var ev = new AttemptCollideSpiritCandleEvent();
        RaiseLocalEvent(args.OtherEntity, ref ev);

        if (ev.Cancelled)
            return;

        if (!_whitelist.IsWhitelistPass(ent.Comp.Whitelist, args.OtherEntity))
            return;

        Log.Info("Removed spirit candle effects");
        ent.Comp.EntitiesInside.Remove(args.OtherEntity);

        _visibility.AddLayer(args.OtherEntity, (int) VisibilityFlags.Ghost, false);
        _visibility.RemoveLayer(args.OtherEntity, (int) VisibilityFlags.Normal, false);
        _visibility.RefreshVisibility(args.OtherEntity);
    }

    private void OnAttemptCollideSpiritCandle(Entity<CorporealComponent> ent, ref AttemptCollideSpiritCandleEvent args) =>
        args.Cancelled = true; // if already corporeal, don't do anything
    #endregion
}

