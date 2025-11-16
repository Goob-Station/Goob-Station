using Content.Shared.Random.Helpers;

namespace Content.Shared._Lavaland.Megafauna.Selectors;

/// <summary>
/// Invokes an attack from one of the child action selectors, based on the weight of the children
/// </summary>
public sealed partial class GroupMegafaunaSelector : MegafaunaSelector
{
    [DataField(required: true)]
    public List<MegafaunaSelector> Children = new();

    protected override float InvokeImplementation(MegafaunaCalculationBaseArgs args)
    {
        var children = new Dictionary<MegafaunaSelector, float>(Children.Count);
        foreach (var child in Children)
        {
            // Don't include invalid groups
            if (!child.CheckConditions(args))
                continue;

            children.Add(child, child.Weight);
        }

        if (children.Count == 0)
            return FailDelay;

        var pick = SharedRandomExtensions.Pick(children, args.Random);
        return pick.Invoke(args);
    }
}
