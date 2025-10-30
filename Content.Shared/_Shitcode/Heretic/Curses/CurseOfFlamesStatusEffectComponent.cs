using Robust.Shared.GameStates;

namespace Content.Shared._Shitcode.Heretic.Curses;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
public sealed partial class CurseOfFlamesStatusEffectComponent : Component
{
    [DataField]
    public float MinFireStacks = 0.5f;

    [DataField, AutoPausedField]
    public TimeSpan NextIgnition = TimeSpan.Zero;

    [DataField]
    public TimeSpan Delay = TimeSpan.FromMilliseconds(1010);
}
