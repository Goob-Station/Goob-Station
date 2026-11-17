using Content.Goobstation.Shared.MartialArts.Components;
using Content.Shared.EntityEffects;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.MartialArts.Effects;

public sealed partial class ApplyMartialArtsMultiplierEntityEffectSystem : EntityEffectSystem<ComboPerformerComponent, ApplyMartialArtsMultiplier>
{
    [Dependency] private MartialArtsSystem _martialArts = default!;

    protected override void Effect(Entity<ComboPerformerComponent> target, ref EntityEffectEvent<ApplyMartialArtsMultiplier> args)
    {
        _martialArts.ApplyMultiplier(
            target,
            args.Effect.Multiplier,
            args.Effect.Modifier,
            args.Effect.Time,
            args.Effect.Type
        );
    }
}

[UsedImplicitly]
public sealed partial class ApplyMartialArtsMultiplier : EntityEffectBase<ApplyMartialArtsMultiplier>
{
    [DataField(required: true)]
    public float Multiplier;

    [DataField(required: true)]
    public float Modifier;

    [DataField(required: true)]
    public TimeSpan Time;

    [DataField(required: true)]
    public MartialArtModifierType Type;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => "Placeholder";
}
