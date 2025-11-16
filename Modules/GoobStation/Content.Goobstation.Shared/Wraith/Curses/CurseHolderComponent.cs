using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wraith.Curses;

/// <summary>
/// This component marks the user as an entity that has Curses on them
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class CurseHolderComponent : Component
{
    /// <summary>
    /// The curses the user currently has.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<ProtoId<CursePrototype>> ActiveCurses = new();

    /// <summary>
    ///  Stores the timespans of the curses
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public Dictionary<ProtoId<CursePrototype>, TimeSpan> CurseUpdate = new();

    [ViewVariables, AutoNetworkedField]
    public List<ProtoId<CurseStatusIconPrototype>> CurseStatusIcons = new();

    /// <summary>
    /// The entity that cursed us
    /// </summary>
    [ViewVariables]
    public EntityUid? Curser;
}
