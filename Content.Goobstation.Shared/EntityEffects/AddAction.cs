using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityEffects;

public sealed partial class AddAction : EntityEffectBase<AddAction>
{
    [DataField]
    public EntProtoId? Action;

    [ViewVariables]
    public EntityUid? ActionId;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("entity-effect-guidebook-add-action", ("chance", Probability));
}

public sealed partial class AddActionEntityEffectSystem : EntityEffectSystem<ActionsComponent, AddAction>
{
    [Dependency] private readonly SharedActionsSystem _action = default!;

    protected override void Effect(Entity<ActionsComponent> entity, ref EntityEffectEvent<AddAction> args)
    {
        if (args.Effect.Action is not { } action)
            return;

        _action.AddAction(entity, ref args.Effect.ActionId, action);
    }
}
