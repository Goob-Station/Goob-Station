namespace Content.Shared.Actions.Events;
public sealed partial class DarkSwapActionEvent : InstantActionEvent
{
    [DataField]
    public bool CheckInsulation;
}
