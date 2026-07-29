using Content.Shared.EntityEffects;

namespace Content.Shared._Trauma.EntityEffects;

/// <summary>
/// Deletes the target entity, be careful with this...
/// </summary>
public sealed partial class Delete : EntityEffectBase<Delete>
{
    [DataField]
    public bool Queued;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("entity-effect-guidebook-delete-entity", ("chance", Probability));
}

public sealed class DeleteEffectSystem : EntityEffectSystem<MetaDataComponent, Delete>
{
    protected override void Effect(Entity<MetaDataComponent> ent, ref EntityEffectEvent<Delete> args)
    {
        if (TerminatingOrDeleted(ent, ent.Comp))
            return;

        var meta = ent.AsNullable();
        if (args.Effect.Queued)
            PredictedQueueDel(meta);
        else
            PredictedDel(meta);
    }
}
