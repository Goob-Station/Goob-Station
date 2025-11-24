using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Morph;

[RegisterComponent, NetworkedComponent]
public sealed partial class MorphDisguiseComponent : Component
{
    /// <summary>
    /// Examine message shown when morphed
    /// </summary>
    public string ExamineMessage = "morph-examine";

    public int Priority = 15;
}
