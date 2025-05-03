namespace Content.Goobstation.Common.Wizard.Targeting;

/// <summary>
/// This handles...
/// </summary>
[Serializable]
public sealed class SetActionTargetMarkEvent(EntityUid? target) : EntityEventArgs
{
    public EntityUid? Target = target;
}
