using Content.Shared.Actions;
using Content.Shared.Inventory;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Funkystation.Clothing.Components;

/// <summary>
/// Gives the wearer an action to toggle this item's <see cref="SubFloor.TrayScannerComponent"/>.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TrayScannerToggleComponent : Component
{
    [DataField]
    public EntProtoId Action = "ActionToggleTrayScanner";

    [DataField, AutoNetworkedField]
    public EntityUid? ActionEntity;

    [DataField]
    public SlotFlags RequiredSlot = SlotFlags.EYES;

    [DataField]
    public SoundSpecifier? SoundOn;

    [DataField]
    public SoundSpecifier? SoundOff;
}

public sealed partial class ToggleTrayScannerEvent : InstantActionEvent;
