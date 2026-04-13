using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._Shitmed.Medical.Surgery.Consciousness.Systems;
using Content.Shared._Shitmed.Medical.Surgery.Pain.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitmed.EntityEffects.Effects;

// Todo this is half-baked yoinkage from Trauma actually fix this I just don't have the time rn.
public sealed partial class SuppressPain : EntityEffectBase<SuppressPain>
{
    [DataField(required: true)]
    public FixedPoint2 Amount = default!;

    [DataField(required: true)]
    public TimeSpan Time = default!;

    [DataField]
    public string ModifierIdentifier = "PainSuppressant";

    /// <summary>
    /// The body part to change the pain for.
    /// </summary>
    [DataField]
    public string OrganCategory = "Torso";

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-suppress-pain");
}

public sealed class SuppressPainEffectSystem : EntityEffectSystem<BodyComponent, SuppressPain>
{
    [Dependency] private readonly ConsciousnessSystem _consciousness = default!;
    [Dependency] private readonly PainSystem _pain = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;

    protected override void Effect(Entity<BodyComponent> ent, ref EntityEffectEvent<SuppressPain> args)
    {
        var scale = FixedPoint2.New(args.Scale);

        if (!_consciousness.TryGetNerveSystem(ent, out var nerveSys))
            return;

        var effect = args.Effect;

        EntityUid? organ = null;
        foreach (var (id, part) in _body.GetBodyChildren(ent.Owner, ent.Comp))
        {
            if (part.PartType.ToString().Equals(effect.OrganCategory, StringComparison.OrdinalIgnoreCase))
            {
                organ = id;
                break;
            }
        }

        if (organ is not { } foundOrgan)
            return;

        var nerves = nerveSys.Value;
        var ident = effect.ModifierIdentifier;
        var amount = effect.Amount * scale;
        var time = effect.Time;

        if (_pain.TryGetPainModifier(nerves, foundOrgan, ident, out var modifier))
            _pain.TryChangePainModifier(nerves, foundOrgan, ident, modifier.Value.Change - amount, time: time);
        else
            _pain.TryAddPainModifier(nerves, foundOrgan, ident, -amount, time: time);
    }
}
