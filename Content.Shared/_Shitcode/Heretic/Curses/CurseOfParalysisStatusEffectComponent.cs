using Robust.Shared.GameStates;

namespace Content.Shared._Shitcode.Heretic.Curses;

[RegisterComponent, NetworkedComponent]
public sealed partial class CurseOfParalysisStatusEffectComponent : Component
{
    [DataField]
    public bool WasParalyzed;
}
