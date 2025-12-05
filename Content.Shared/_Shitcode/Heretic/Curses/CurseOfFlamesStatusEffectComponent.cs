using Robust.Shared.GameStates;

namespace Content.Shared._Shitcode.Heretic.Curses;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
public sealed partial class CurseOfFlamesStatusEffectComponent : Component
{
    [DataField]
    public float MinFireStacks = 2f;

    [DataField]
    public float Penetration = 0.5f;

    [DataField, AutoPausedField]
    public TimeSpan NextIgnition = TimeSpan.Zero;

    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(10);
}
