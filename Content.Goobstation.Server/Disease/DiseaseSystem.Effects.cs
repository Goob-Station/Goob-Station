using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Disease;
using Content.Goobstation.Shared.Disease.Components;
using Content.Server.Chat.Systems;
using Content.Shared.EntityEffects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Numerics;

namespace Content.Goobstation.Server.Disease;

public sealed partial class DiseaseSystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    // cache for field setters for DiseaseGenericEffectComponent
    private readonly Dictionary<(Type, string), Action<Component, float>> _setterCache = new();

    protected override void InitializeEffects()
    {
        base.InitializeEffects();

        SubscribeLocalEvent<DiseaseReagentEffectComponent, DiseaseEffectEvent>(OnReagentEffect); // can get moved to shared after we get shared entity effects
        SubscribeLocalEvent<DiseaseEmoteEffectComponent, DiseaseEffectEvent>(OnEmoteEffect);
        SubscribeLocalEvent<DiseaseGenericEffectComponent, DiseaseEffectEvent>(OnGenericEffect);
        SubscribeLocalEvent<DiseaseGenericEffectComponent, DiseaseEffectFailedEvent>(OnGenericEffectFail);
    }

    private void OnReagentEffect(Entity<DiseaseReagentEffectComponent> ent, ref DiseaseEffectEvent args)
    {
        var reagentArgs = new EntityEffectReagentArgs(
            targetEntity: args.Ent,
            entityManager: EntityManager,
            organEntity: null,
            source: null,
            quantity: FixedPoint2.New(1),
            reagent: null,
            method: null,
            scale: FixedPoint2.New(GetScale(args, ent.Comp))
        );

        foreach (var effect in ent.Comp.Effects)
        {
            if (effect.ShouldApply(reagentArgs, _random))
                effect.Effect(reagentArgs);
        }
    }

    private void OnEmoteEffect(Entity<DiseaseEmoteEffectComponent> ent, ref DiseaseEffectEvent args)
    {
        var emote = _proto.Index(ent.Comp.Emote);
        if (ent.Comp.WithChat)
            _chat.TryEmoteWithChat(args.Ent, emote);
        else
            _chat.TryEmoteWithoutChat(args.Ent, emote);
    }

    private void OnGenericEffect(Entity<DiseaseGenericEffectComponent> ent, ref DiseaseEffectEvent args)
    {
        ApplyGenericEffect(ent, GetScale(args, ent.Comp));
    }

    private void OnGenericEffectFail(Entity<DiseaseGenericEffectComponent> ent, ref DiseaseEffectFailedEvent args)
    {
        if (ent.Comp.ZeroOnFail)
            ApplyGenericEffect(ent, 0f);
    }

    public void ApplyGenericEffect(Entity<DiseaseGenericEffectComponent> ent, float mul)
    {
        if (!Factory.TryGetRegistration(ent.Comp.Component, out var registration))
        {
            Log.Error($"Unknown target component '{ent.Comp.Component}' on {ToPrettyString(ent)}");
            return;
        }

        var targetType = registration.Type;
        if (!EntityManager.TryGetComponent(ent, Factory.GetRegistration(targetType), out var comp))
            return;

        foreach (var (field, baseValue) in ent.Comp.Defaults)
        {
            var key = (targetType, field);

            if (!_setterCache.TryGetValue(key, out var setter))
            {
                setter = CompileSetter(targetType, field);
                _setterCache[key] = setter;
            }

            setter?.Invoke((Component)comp, baseValue * mul);
        }
    }

    /// <summary>
    /// Compiles a lambda: (Component target, float value) => ((TargetType)target).FieldName = value;
    /// </summary>
    private Action<Component, float> CompileSetter(Type targetType, string fieldName)
    {
        var targetParam = Expression.Parameter(typeof(Component), "target");
        var valueParam = Expression.Parameter(typeof(float), "value");
        var castParam = Expression.Convert(targetParam, targetType);

        var memberAccess = Expression.PropertyOrField(castParam, fieldName);

        DebugTools.Assert(memberAccess.Type == typeof(float));

        var assign = Expression.Assign(memberAccess, valueParam);

        return Expression.Lambda<Action<Component, float>>(assign, targetParam, valueParam).Compile();
    }

    #region public API

    /// <summary>
    /// Adds an effect of given prototype to the specified disease
    /// </summary>
    public override bool TryAddEffect(Entity<DiseaseComponent?> ent, EntProtoId effectId, [NotNullWhen(true)] out Entity<DiseaseEffectComponent>? effect)
    {
        effect = null;
        if (!Resolve(ent, ref ent.Comp) || HasEffect(ent, effectId))
            return false;

        var effectUid = Spawn(effectId, new EntityCoordinates(ent, Vector2.Zero));
        if (!TryAddEffect(ent, effectUid, out effect))
        {
            QueueDel(effectUid);
            return false;
        }

        Dirty(ent);
        return true;
    }

    /// <summary>
    /// Removes the specified disease effect from this disease
    /// </summary>
    public override bool TryRemoveEffect(Entity<DiseaseComponent?> ent, EntityUid effect)
    {
        if (!Resolve(ent, ref ent.Comp))
            return false;

        if (ent.Comp.Effects.Remove(effect))
            QueueDel(effect);
        else
            return false;

        Dirty(ent);
        return true;
    }

    #endregion
}
