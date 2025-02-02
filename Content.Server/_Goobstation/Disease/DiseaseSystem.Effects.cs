using Content.Server.Flash;
using Content.Server.Medical;
using Content.Shared.Disease;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Content.Server.Disease;

public sealed partial class DiseaseSystem
{
    [Dependency] private readonly VomitSystem _vomit = default!;
    [Dependency] private readonly FlashSystem _flash = default!;

    protected override void InitializeEffects()
    {
        base.InitializeEffects();

        SubscribeLocalEvent<DiseaseVomitEffectComponent, DiseaseEffectEvent>(OnVomitEffect);
        SubscribeLocalEvent<DiseaseFlashEffectComponent, DiseaseEffectEvent>(OnFlashEffect);
    }

    private void OnVomitEffect(EntityUid uid, DiseaseVomitEffectComponent effect, DiseaseEffectEvent args)
    {
        _vomit.Vomit(args.Ent, effect.ThirstChange * GetScale(args, effect), effect.FoodChange * GetScale(args, effect));
    }

    private void OnFlashEffect(EntityUid uid, DiseaseFlashEffectComponent effect, DiseaseEffectEvent args)
    {
        TimeSpan? stunDuration = effect.StunDuration == null ? null : (effect.StunDuration * GetScale(args, effect));
        _flash.Flash(args.Ent, null, null, effect.Duration * GetScale(args, effect), effect.SlowTo, stunDuration: stunDuration);
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
