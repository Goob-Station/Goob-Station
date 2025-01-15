namespace Content.Shared._Goobstation.PrisonerId;

/// <summary>
/// This handles...
/// </summary>
public sealed class PrisonerIdSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<PrisonerIdComponent,ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, PrisonerIdComponent component, ComponentStartup args)
    {
        throw new NotImplementedException();
    }
}
