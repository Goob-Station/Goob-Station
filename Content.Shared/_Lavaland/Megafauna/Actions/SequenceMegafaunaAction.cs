using Content.Shared._Lavaland.Megafauna.NumberSelectors;
using Content.Shared._Lavaland.Megafauna.Systems;

namespace Content.Shared._Lavaland.Megafauna.Actions;

/// <summary>
/// Runs specified megafauna action multiple times with some delay.
/// </summary>
public sealed partial class SequenceMegafaunaAction : MegafaunaActionSelector
{
    [DataField(required: true)]
    public MegafaunaActionSelector Selector;

    /// <summary>
    /// This delay specifies how often Selector is being added to the schedule.
    /// It's a different thing from DelaySelector and should always
    /// </summary>
    [DataField("sequenceDelay",required: true)]
    public MegafaunaNumberSelector BetweenSequenceDelay;

    /// <summary>
    /// Total amount of selectors to add.
    /// </summary>
    [DataField]
    public MegafaunaNumberSelector Rolls = new MegafaunaConstantNumberSelector(1);

    protected override float InvokeImplementation(MegafaunaCalculationBaseArgs args)
    {
        var megafaunaSys = args.EntityManager.System<SharedMegafaunaSystem>();

        var rolls = Rolls.GetRounded(args);
        var delay = BetweenSequenceDelay.Get(args);

        Selector.CopyFrom(this);
        Selector.IsDeadEnd = true;

        for (int i = 0; i < rolls; i++)
        {
            Selector.Counter = i;
            megafaunaSys.AddMegafaunaAction(args.AiComponent, Selector, delay * (i + 1));
        }

        Logger.Info($"Sequence Action spawned {Selector} with amount {rolls}");

        if (rolls == 0)
            return FailDelay;

        return DelaySelector.Get(args);
    }
}
