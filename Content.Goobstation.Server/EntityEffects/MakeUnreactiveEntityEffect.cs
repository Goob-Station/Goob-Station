using Content.Shared.Chemistry.Reaction;
using Content.Shared.EntityEffects;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.EntityEffects;

public sealed partial class MakeUnreactiveEntityEffect : EntityEffect
{
    private static readonly ProtoId<TagPrototype> TrashTag = "Trash";

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => null;

    public override void Effect(EntityEffectBaseArgs args)
    {
        // whatever bro
        args.EntityManager.RemoveComponent<ReactiveComponent>(args.TargetEntity);
        args.EntityManager.System<TagSystem>().AddTag(args.TargetEntity, TrashTag);
    }
}
