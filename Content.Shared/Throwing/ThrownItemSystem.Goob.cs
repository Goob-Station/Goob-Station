namespace Content.Shared.Throwing;

public sealed partial class ThrownItemSystem
{
    private readonly List<(EntityUid Thrown, EntityUid Target)> _deferredCollisions = new();

    private bool TryDeferCollision(EntityUid thrown, EntityUid target)
    {
        if (!_netMan.IsClient || !_gameTiming.ApplyingState)
            return false;

        _deferredCollisions.Add((thrown, target));
        return true;
    }

    private void ProcessDeferredCollisions()
    {
        if (_deferredCollisions.Count == 0)
            return;

        var collisions = _deferredCollisions.ToArray();
        _deferredCollisions.Clear();
        foreach (var (thrown, target) in collisions)
        {
            if (TerminatingOrDeleted(thrown) || TerminatingOrDeleted(target) ||
                !TryComp<ThrownItemComponent>(thrown, out var component))
                continue;

            ThrowCollideInteraction(component, thrown, target);
        }
    }
}
