using System.Linq;

namespace Content.Shared._Lavaland.Megafauna.Selectors;

/// <summary>
/// Invokes all children attacks at once.
/// </summary>
public sealed partial class AllMegafaunaSelector : MegafaunaSelector
{
    [DataField(required: true)]
    public List<MegafaunaSelector> Children = new();

    protected override float InvokeImplementation(MegafaunaCalculationBaseArgs args)
    {
        var maxDelay = FailDelay;
        var sortedChildren = Children.OrderBy(m => m.Priority).ToList();

        foreach (var child in sortedChildren)
        {
            var delay = child.Invoke(args);
            if (delay > maxDelay)
                maxDelay = delay;
        }

        return maxDelay;
    }
}
