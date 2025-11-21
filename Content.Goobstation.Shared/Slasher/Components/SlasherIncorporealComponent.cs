using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Slasher.Components;

/// <summary>
/// Grants the Slasher the ability to toggle incorporeal form.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class SlasherIncorporealComponent : Component
{
    [ViewVariables]
    public EntityUid? IncorporealizeActionEnt;

    [ViewVariables]
    public EntityUid? CorporealizeActionEnt;

    [DataField]
    public EntProtoId IncorporealizeActionId = "ActionSlasherIncorporealize";

    [DataField]
    public EntProtoId CorporealizeActionId = "ActionSlasherCorporealize";

    /// <summary>
    /// Current state of the slasher. True when incorporeal.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsIncorporeal;

    /// <summary>
    /// Range (in tiles) to check for observers with line of sight that prevent incorporealizing.
    /// </summary>
    [DataField]
    public float ObserverCheckRange = 10f;

    /// <summary>
    /// How long the do-after to enter incorporeal form takes.
    /// </summary>
    [DataField]
    public TimeSpan IncorporealizeDelay = TimeSpan.FromSeconds(2);
}
