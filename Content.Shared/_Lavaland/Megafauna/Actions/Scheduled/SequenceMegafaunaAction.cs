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
    /// Modifies Counter on an action based on this function, if specified.
    /// </summary>
    [DataField]
    public MegafaunaNumberSelector CounterModifier = new MegafaunaConstantNumberSelector(1f);

    /// <summary>
    /// Total amount of selectors to add.
    /// </summary>
    [DataField]
    public MegafaunaNumberSelector Rolls = new MegafaunaConstantNumberSelector(1);

    [DataField]
    public int StartCounter = 1;

    protected override float InvokeImplementation(MegafaunaCalculationBaseArgs args)
    {
        var megafaunaSys = args.EntityManager.System<SharedMegafaunaSystem>();
        var comp = args.AiComponent;

        var rolls = Rolls.GetRounded(args);
        if (rolls <= 0)
            return FailDelay;

        var delay = Selector.DelaySelector.Get(args);
        if (delay <= 0f)
            return FailDelay;

        Selector.CopyFrom(this);

        var thread = megafaunaSys.AddNewThread(comp);
        for (var i = StartCounter; i < rolls; i++)
        {
            // TODO MEGAFAUNA remove serial man and check if it works
            MegafaunaActionSelector? action = null;
            args.SerialMan.CopyTo(Selector, ref action);
            if (action == null)
                continue;

            CounterModifier.Value = i;
            var counter = CounterModifier.GetRounded(args);

            if (IsSequence is true)
                action.Counter = counter;

            megafaunaSys.AddActionToThread(comp, thread, action, delay * (i + 1));
        }

        return delay * rolls;
    }
}
