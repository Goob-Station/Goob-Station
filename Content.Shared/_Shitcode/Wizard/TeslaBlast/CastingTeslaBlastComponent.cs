using Robust.Shared.Audio.Components;
using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Wizard.TeslaBlast;

[RegisterComponent, NetworkedComponent]
public sealed partial class CastingTeslaBlastComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public ushort DoAfterId;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? Effect;

    [ViewVariables(VVAccess.ReadOnly)]
    public Entity<AudioComponent>? Sound;
}
