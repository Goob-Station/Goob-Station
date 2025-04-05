using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Medical.Components;

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

    /// <summary>
    /// Color that will be used on target entity when healing is active.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Color LineColor;
}
