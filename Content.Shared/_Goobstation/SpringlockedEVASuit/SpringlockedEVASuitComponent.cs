using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.SpringlockedEVASuit;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SpringlockedEVASuitComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool IsSpringed;

    [DataField, AutoNetworkedField]
    public EntityUid? Wearer;

    [DataField]
    public SoundSpecifier SnapSound { get; set; } = new SoundPathSpecifier("/Audio/Items/Handcuffs/cuff_start.ogg"); // should probably use different sounds.

    [DataField, AutoNetworkedField]
    public float UpdateTimer = 0f;

    [DataField, AutoNetworkedField]
    public float UpdateDelay = 1.5f;
}
