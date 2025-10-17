using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Wizard.EventSpells;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MobCollisionSpellRuleComponent : Component
{

    public bool WasEnabled;
}
