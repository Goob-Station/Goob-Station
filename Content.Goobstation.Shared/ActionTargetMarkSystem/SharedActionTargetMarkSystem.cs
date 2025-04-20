namespace Content.Goobstation.Shared.ActionTargetMarkSystem;

/// <summary>
/// This handles...
/// </summary>
public abstract class SharedActionTargetMarkSystem : EntitySystem, IActionTargetMarkSystem
{
    public EntityUid? Target { get; protected set; }
    public EntityUid? Mark { get; protected set; }
    public virtual void SetMark(EntityUid? uid)
    {
    }
}
