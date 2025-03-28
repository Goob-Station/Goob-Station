using Content.Server.Chat.Systems;
using Content.Server.Medical;
using Content.Goobstation.Shared.Disease;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Content.Goobstation.Server.Disease;

public sealed partial class DiseaseSystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly VomitSystem _vomit = default!;

    protected override void InitializeEffects()
    {
        base.InitializeEffects();

        SubscribeLocalEvent<DiseaseEmoteEffectComponent, DiseaseEffectEvent>(OnEmoteEffect);
        SubscribeLocalEvent<DiseaseVomitEffectComponent, DiseaseEffectEvent>(OnVomitEffect);
    }

    private void OnEmoteEffect(EntityUid uid, DiseaseEmoteEffectComponent effect, DiseaseEffectEvent args)
    {
        var emote = _proto.Index(effect.Emote);
        if (effect.WithChat)
            _chat.TryEmoteWithChat(args.Ent, emote);
        else
            _chat.TryEmoteWithoutChat(args.Ent, emote);
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
