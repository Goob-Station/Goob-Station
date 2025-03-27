namespace Content.Shared.Rejuvenate;

public sealed class RejuvenateEvent(bool uncuff = true, bool resetActions = true) : EntityEventArgs // Goob edit
{
    // Goobstation start
    public bool Uncuff = uncuff;

    public bool ResetActions = resetActions;
    // Goobstation end
}
