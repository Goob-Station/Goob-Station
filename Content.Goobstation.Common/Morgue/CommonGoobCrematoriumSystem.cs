namespace Content.Goobstation.Common.Morgue;

// Le experimental way to communicate between core and custom
public abstract class CommonGoobCrematoriumSystem : EntitySystem
{
    public abstract bool CanCremate(EntityUid target);
    public abstract void TryDeleteItems(EntityUid target, EntityUid crematorium);
}
