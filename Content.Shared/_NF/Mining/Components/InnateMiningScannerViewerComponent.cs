using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Content.Shared.Mining;

namespace Content.Shared._NF.Mining.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(MiningScannerSystem))]
public sealed partial class InnateMiningScannerViewerComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public float ViewRange;

    [DataField, AutoNetworkedField]
    public float AnimationDuration = 1.5f;

    [DataField, AutoNetworkedField]
    public TimeSpan PingDelay = TimeSpan.FromSeconds(5);

    [DataField, AutoNetworkedField]
    public SoundSpecifier? PingSound = null;

}
