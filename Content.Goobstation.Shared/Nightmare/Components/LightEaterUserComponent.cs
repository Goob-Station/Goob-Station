using Robust.Shared.GameStates;


namespace Content.Shared._EE.Nightmare.Components;


/// <summary>
/// This is used for indicating that the user owns this action
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class LightEaterUserComponent : Component
{

    [DataField]
    public string? LightEaterProto = "LightEaterArmBlade";

    [DataField]
    public bool Activated;

    [DataField]
    public EntityUid? LightEaterEntity;
}
