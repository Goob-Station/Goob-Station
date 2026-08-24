using Content.Shared.Chemistry.Reaction;
using Content.Shared.EntityEffects;
using Content.Shared.EntityEffects.Effects;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityEffects;

public sealed partial class MakeUnreactiveEntityEffectSystem
    : EntityEffectSystem<ReactiveComponent, MakeUnreactiveEntityEffect>
{
    private static readonly ProtoId<TagPrototype> TrashTag = "Trash";

    [Dependency] private readonly TagSystem _tags = default!;

    protected override void Effect(Entity<ReactiveComponent> entity, ref EntityEffectEvent<MakeUnreactiveEntityEffect> args)
    {
        // ?????
        RemComp<ReactiveComponent>(entity.Owner);
        _tags.AddTag(entity.Owner, TrashTag);
    }
}

public sealed partial class MakeUnreactiveEntityEffect : EntityEffectBase<MakeUnreactiveEntityEffect>
{
    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => null;
}
