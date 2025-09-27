using Content.Shared.Actions;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Clothing.Components;

/// <summary>
/// This is used for imortality talisman item
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ImortalityTalismanComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? ActionEntity;

    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(10);

    [DataField]
    public TimeSpan DisableAt = TimeSpan.Zero;

    [DataField]
    public TimeSpan Cooldown = TimeSpan.Zero;

    [DataField]
    public TimeSpan CooldownDuration = TimeSpan.FromSeconds(60);

    public bool Active;

    public EntityUid? EntityGrantedImortality;
}

[ByRefEvent]
public sealed partial class ActivateImortalityTalismanActionEvent : InstantActionEvent;
