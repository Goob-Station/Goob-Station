using Content.Goobstation.Shared.Mech.Systems;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Mech.Components;

/// <summary>
/// Removes power from any powersources that are inserted into an entity with this component.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ZapPowerOnInsertComponent : Component
{
    [DataField(required: true), AutoNetworkedField]
    public string ContainerId = string.Empty;

    [DataField(required: true), AutoNetworkedField]
    public float ChargeLoss;

    [DataField, AutoNetworkedField]
    public SoundSpecifier? ZapSound = new SoundCollectionSpecifier("sparks");
}
