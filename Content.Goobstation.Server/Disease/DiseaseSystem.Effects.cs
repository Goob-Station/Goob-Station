using Content.Server.Chat.Systems;
using Content.Goobstation.Shared.Disease;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Disease.Components;
using Content.Shared.EntityEffects;

namespace Content.Goobstation.Server.Disease;

public sealed partial class DiseaseSystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    protected override void InitializeEffects()
    {
        base.InitializeEffects();

        SubscribeLocalEvent<DiseaseReagentEffectComponent, DiseaseEffectEvent>(OnReagentEffect); // can get moved to shared after we get shared entity effects
        SubscribeLocalEvent<DiseaseEmoteEffectComponent, DiseaseEffectEvent>(OnEmoteEffect);
    }

    private void OnReagentEffect(EntityUid uid, DiseaseReagentEffectComponent reagentEffect, DiseaseEffectEvent args)
    {
        var reagentArgs = new EntityEffectReagentArgs(
            targetEntity: args.Ent,
            entityManager: EntityManager,
            organEntity: null,
            source: null,
            quantity: FixedPoint2.New(1),
            reagent: null,
            method: null,
            scale: reagentEffect.Scale ? FixedPoint2.New(GetScale(args, reagentEffect)) : FixedPoint2.New(1)
        );

        foreach (var effect in reagentEffect.Effects)
        {
            if (effect.ShouldApply(reagentArgs, _random))
                effect.Effect(reagentArgs);
        }
    }
    private void OnEmoteEffect(EntityUid uid, DiseaseEmoteEffectComponent effect, DiseaseEffectEvent args)
    {
        var emote = _proto.Index(effect.Emote);
        if (effect.WithChat)
            _chat.TryEmoteWithChat(args.Ent, emote);
        else
            _chat.TryEmoteWithoutChat(args.Ent, emote);
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
