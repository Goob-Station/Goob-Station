namespace Content.Server._Goobstation.Wizard.Components;

/// <summary>
/// Changes action's use delay on map init. Used for easier wizard spell upgrades in yaml
/// </summary>
[RegisterComponent]
public sealed partial class ActionUseDelayModifierComponent : Component
{
    [DataField(required: true)]
    public TimeSpan? UseDelay;
}
