using Content.Goobstation.Shared.Wizard.SpellCards;

namespace Content.Goobstation.Shared.ActionTargetMarkSystem;

/// <summary>
/// This handles...
/// </summary>
[Serializable]
public sealed class SetActionTargetMarkEvent(EntityUid? target) : EntityEventArgs
{
    public EntityUid? Target = target;
}
