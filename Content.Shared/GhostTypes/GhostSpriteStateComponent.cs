<<<<<<<< HEAD:Content.Shared/Ghost/GhostSpriteStateSelection/GhostSpriteStateComponent.cs
namespace Content.Shared.Ghost.GhostSpriteStateSelection;
========
using Robust.Shared.GameStates;

namespace Content.Shared.GhostTypes;
>>>>>>>> 75fca03bb1 (renaming and moving stuff to shared):Content.Shared/GhostTypes/GhostSpriteStateComponent.cs

/// <summary>
/// Changes the entity sprite according to damage taken
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class GhostSpriteStateComponent : Component
{
    /// <summary>
    /// Prefix the GhostSpriteStateSystem will add to the name of the damage type it chooses.
    /// It should be identical to the prefix of the entity optional damage sprites.
    /// </summary>
    [DataField]
    public string prefix = "";
}
