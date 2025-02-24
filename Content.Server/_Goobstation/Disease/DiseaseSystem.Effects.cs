using Content.Server.Medical;
using Content.Shared.Disease;
using Content.Shared.Flash;
using Content.Shared.Flash.Components;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Content.Server.Disease;

public sealed partial class DiseaseSystem
{
    [Dependency] private readonly SharedFlashSystem _flash = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly VomitSystem _vomit = default!;

    protected override void InitializeEffects()
    {
        base.InitializeEffects();

        SubscribeLocalEvent<DiseaseFlashEffectComponent, DiseaseEffectEvent>(OnFlashEffect);
        SubscribeLocalEvent<DiseasePopupEffectComponent, DiseaseEffectEvent>(OnPopupEffect);
        SubscribeLocalEvent<DiseaseVomitEffectComponent, DiseaseEffectEvent>(OnVomitEffect);
    }

    private void OnFlashEffect(EntityUid uid, DiseaseFlashEffectComponent effect, DiseaseEffectEvent args)
    {
        _status.TryAddStatusEffect<FlashedComponent>(args.Ent, _flash.FlashedKey, effect.Duration * GetScale(args, effect), true);
        _stun.TrySlowdown(args.Ent, effect.Duration * GetScale(args, effect), true, effect.SlowTo, effect.SlowTo);

        if (effect.StunDuration != null)
            _stun.TryKnockdown(args.Ent, effect.StunDuration.Value * GetScale(args, effect), true);
    }

    private void OnPopupEffect(EntityUid uid, DiseasePopupEffectComponent effect, DiseaseEffectEvent args)
    {
        _popup.PopupEntity(Loc.GetString(effect.String), args.Ent, args.Ent, effect.Type);
    }

    private void OnVomitEffect(EntityUid uid, DiseaseVomitEffectComponent effect, DiseaseEffectEvent args)
    {
        _vomit.Vomit(args.Ent, effect.ThirstChange * GetScale(args, effect), effect.FoodChange * GetScale(args, effect));
    }

    #region public API

    /// <summary>
    /// Adds an effect of given prototype to the specified disease
    /// </summary>
    public override bool TryAddEffect(EntityUid uid, EntProtoId effectId, [NotNullWhen(true)] out Entity<DiseaseEffectComponent>? effect, DiseaseComponent? comp = null)
    {
        effect = null;
        if (!Resolve(uid, ref comp) || HasEffect(uid, effectId, comp))
            return false;

        var effectUid = Spawn(effectId, new EntityCoordinates(uid, Vector2.Zero));
        if (!TryAddEffect(uid, effectUid, out effect, comp))
        {
            QueueDel(effectUid);
            return false;
        }

        Dirty(uid, comp);
        return true;
    }

    /// <summary>
    /// Removes the specified disease effect from this disease
    /// </summary>
    public override bool TryRemoveEffect(EntityUid uid, EntityUid effect, DiseaseComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return false;

        if (comp.Effects.Remove(effect))
            QueueDel(effect);
        else
            return false;

        Dirty(uid, comp);
        return true;
    }

    #endregion
}
