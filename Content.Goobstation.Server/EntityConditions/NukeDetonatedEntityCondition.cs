using Content.Server.Nuke;
using Content.Shared.EntityConditions;
using Content.Shared.GameTicking;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.EntityConditions;

public sealed partial class NukeDetonatedEntityConditionSystem : EntityConditionSystem<TransformComponent, NukeDetonatedCondition>
{
    private bool _detonated;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NukeExplodedEvent>(OnNukeExploded);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
    }

    private void OnNukeExploded(NukeExplodedEvent ev)
    {
        _detonated = true;
    }

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        _detonated = false;
    }

    protected override void Condition(Entity<TransformComponent> entity, ref EntityConditionEvent<NukeDetonatedCondition> args)
    {
        args.Result = _detonated;
    }
}

public sealed partial class NukeDetonatedCondition : EntityConditionBase<NukeDetonatedCondition>
{
    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        return string.Empty;
    }
}
