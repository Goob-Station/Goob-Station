using System.Linq;

namespace Content.Shared._Lavaland.Megafauna.Actions;

/// <summary>
/// Invokes all children attacks at once.
/// </summary>
public sealed partial class AllMegafaunaAction : MegafaunaActionSelector
{
    [DataField(required: true)]
    public List<MegafaunaActionSelector> Children = new();

    protected override float InvokeImplementation(MegafaunaCalculationBaseArgs args)
    {
        var maxDelay = FailDelay;
        var sortedChildren = Children.OrderBy(m => m.Priority).ToList();

        foreach (var child in sortedChildren)
        {
            child.CopyFrom(this);
            var delay = child.Invoke(args);
            if (delay > maxDelay)
                maxDelay = delay;
        }

        return maxDelay;
    }
}
