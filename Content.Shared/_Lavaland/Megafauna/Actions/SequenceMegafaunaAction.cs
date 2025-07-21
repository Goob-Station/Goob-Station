using Content.Shared._Lavaland.Megafauna.NumberSelectors;

namespace Content.Shared._Lavaland.Megafauna.Actions;

/// <summary>
/// Runs multiple megafauna actions with scheduled delay between them.
/// </summary>
public sealed partial class SequenceMegafaunaAction : MegafaunaActionSelector
{
    [DataField(required: true)]
    public MegafaunaActionSelector Selector;

    /// <summary>
    /// Total amount of selectors to add.
    /// </summary>
    [DataField]
    public MegafaunaNumberSelector Rolls = new MegafaunaConstantNumberSelector(1);

    protected override float InvokeImplementation(MegafaunaCalculationBaseArgs args)
    {
        var rolls = Rolls.GetRounded(args);
        var delay = DelaySelector.Get(args);
        for (int i = 0; i < rolls; i++)
        {
            var time = args.Timing.CurTime + TimeSpan.FromSeconds(delay * i);
            Selector.Counter = i;
            Selector.IsSequence = IsSequence;
            args.AiComponent.ActionSchedule.Add(time, Selector);
        }

        if (rolls == 0)
            return FailDelay;

        return delay * rolls;
    }
}
