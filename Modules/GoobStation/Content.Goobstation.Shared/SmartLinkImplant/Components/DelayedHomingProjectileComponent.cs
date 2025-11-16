using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.SmartLinkImplant;

[RegisterComponent, NetworkedComponent]
public sealed partial class DelayedHomingProjectileComponent : Component
{
    public TimeSpan HomingStart = TimeSpan.Zero;

    public EntityUid Target;
}
