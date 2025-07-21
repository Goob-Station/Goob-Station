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
        foreach (var child in Children)
        {
            var delay = child.Invoke(args, IsSequence, Counter);
            if (delay > maxDelay)
                maxDelay = delay;
        }

        return maxDelay;
    }
}
