namespace Content.Goobstation.Shared.Spy;

/// <summary>
/// This handles...
/// </summary>
public abstract class SharedSpyBountySystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {

    }
    public virtual void SetupBounties() {}
    public virtual void CreateDbEntity() {}
}
