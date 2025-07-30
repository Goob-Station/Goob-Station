using Content.Shared._Lavaland.Aggression;

namespace Content.Shared._Lavaland.Megafauna.Actions;

/// <summary>
/// Picks some target for this megafauna.
/// </summary>
public sealed partial class PickTargetAction : MegafaunaActionSelector
{
    // TODO add something like EntityConditions that check if entity falls under some condition before picking it, so its not just random.
    protected override float InvokeImplementation(MegafaunaCalculationBaseArgs args)
    {
        var entMan = args.EntityManager;
        var uid = args.BossEntity;
        var ai = args.AiComponent;
        var agressionSys = entMan.System<AggressorsSystem>();

        if (!entMan.TryGetComponent(uid, out AggressiveComponent? aggressive))
            return FailDelay;

        ai.PreviousTarget = ai.CurrentTarget;
        agressionSys.TryPickTarget((uid, aggressive), out ai.CurrentTarget);

        return DelaySelector.Get(args);
    }
}
