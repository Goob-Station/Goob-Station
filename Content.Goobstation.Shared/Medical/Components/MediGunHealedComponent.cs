using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Medical.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MediGunHealedComponent : Component
{
    /// <summary>
    /// Source of the healing that it receives.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid Source;

    /// <summary>
    /// Entity meant for medical beam visuals.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? DummyEntity;
}
