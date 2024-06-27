using Robust.Shared.GameStates;

namespace Content.Shared.Goobstation.Changeling;

[RegisterComponent, NetworkedComponent, Access(typeof(ChangelingSystem))]
public sealed partial class ChangelingComponent : Component
{
}
