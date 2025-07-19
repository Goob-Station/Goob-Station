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
        var totalDelay = 0f;
        foreach (var child in Children)
        {
            totalDelay += child.Invoke(args, Counter);
        }

        return totalDelay;
    }
}
