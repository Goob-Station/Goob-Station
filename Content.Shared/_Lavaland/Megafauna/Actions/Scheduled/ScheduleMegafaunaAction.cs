using Content.Shared._Lavaland.Megafauna.Systems;

namespace Content.Shared._Lavaland.Megafauna.Actions;

/// <summary>
/// Adds multiple megafauna action to a schedule with specified delays between them.
/// </summary>
public sealed partial class ScheduleMegafaunaAction : MegafaunaActionSelector
{
    [DataField(required: true)]
    public Dictionary<float, MegafaunaActionSelector> Schedule;

    protected override float InvokeImplementation(MegafaunaCalculationBaseArgs args)
    {
        if (Schedule.Count == 0)
            return FailDelay;

        var megafaunaSys = args.EntityManager.System<SharedMegafaunaSystem>();
        var comp = args.AiComponent;
        var thread = megafaunaSys.AddNewThread(comp);

        foreach (var (delay, selector) in Schedule)
        {
            selector.CopyFrom(this);

            MegafaunaActionSelector? action = null;
            args.SerialMan.CopyTo(selector, ref action);
            megafaunaSys.AddActionToThread(args.AiComponent, thread, action ?? selector, delay);
        }

        return DelaySelector.Get(args);
    }
}
