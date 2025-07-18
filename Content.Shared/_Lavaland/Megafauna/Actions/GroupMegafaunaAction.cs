using Content.Shared.Random.Helpers;

namespace Content.Shared._Lavaland.Megafauna.Actions;

/// <summary>
/// Invokes an attack from one of the child action selectors, based on the weight of the children
/// </summary>
public sealed partial class GroupMegafaunaAction : MegafaunaActionSelector
{
    [DataField(required: true)]
    public List<MegafaunaActionSelector> Children = new();

    protected override float InvokeImplementation(MegafaunaCalculationBaseArgs args)
    {
        var children = new Dictionary<MegafaunaActionSelector, float>(Children.Count);
        foreach (var child in Children)
        {
            // Don't include invalid groups
            if (!child.CheckConditions(args))
                continue;

            children.Add(child, child.Weight);
        }

        var pick = SharedRandomExtensions.Pick(children, args.Random);

        return pick.Invoke(args, Counter);
    }
}
