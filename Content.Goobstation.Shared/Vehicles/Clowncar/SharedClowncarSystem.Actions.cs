namespace Content.Goobstation.Shared.Vehicles.Clowncar;

public abstract partial class SharedClowncarSystem
{
    /// <summary>
    /// Handles activating/deactivating the cannon when requested
    /// </summary>
    private void OnClowncarFireModeAction(EntityUid uid, ClowncarComponent component, ClowncarFireModeActionEvent args)
    {
        if (args.Handled)
            return;

        ToggleCannon(uid, component, args.Performer, true);//component.CannonEntity == null);
        args.Handled = true;
    }
}
