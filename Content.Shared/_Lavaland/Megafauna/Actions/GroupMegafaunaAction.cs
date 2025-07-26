using Content.Shared.Random.Helpers;

namespace Content.Shared._Lavaland.Megafauna.Actions;

/// <summary>
/// Invokes an attack from one of the child action selectors, based on the weight of the children
/// </summary>
public sealed partial class GroupMegafaunaAction : MegafaunaActionSelector
{
    [DataField(required: true)]
    public List<MegafaunaActionSelector> Children = new();

    /// <summary>
    /// While true, prevents attacks from repeating for
    /// their delay multiplied by <see cref="LocalCooldownMultiplier"/>.
    /// </summary>
    [DataField("cooldown")]
    public bool LocalCooldown;

    [DataField]
    public float LocalCooldownMultiplier = 2f;

    [ViewVariables]
    private readonly Dictionary<MegafaunaActionSelector, TimeSpan> _cooldown = new();

    protected override float InvokeImplementation(MegafaunaCalculationBaseArgs args)
    {
        var curTime = args.Timing.CurTime;
        foreach (var (action, _) in _cooldown)
        {
            if (_cooldown[action] < curTime)
                _cooldown.Remove(action);
        }

        var children = new Dictionary<MegafaunaActionSelector, float>(Children.Count);
        foreach (var child in Children)
        {
            // Don't include invalid/cooldowned groups
            if (_cooldown.ContainsKey(child)
                || !child.CheckConditions(args))
                continue;

            children.Add(child, child.Weight);
        }

        if (children.Count == 0)
            return FailDelay;

        var pick = SharedRandomExtensions.Pick(children, args.Random);
        pick.CopyFrom(this);

        var time = pick.Invoke(args);

        if (LocalCooldown)
            _cooldown.Add(pick, TimeSpan.FromSeconds(time) * LocalCooldownMultiplier + curTime);

        return time;
    }
}
