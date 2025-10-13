using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.ModSuits;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ModSuitModComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool IsInstantlyActive = false;

    [AutoNetworkedField]
    public bool Active = false;

    /// <summary>
    ///     Module  limit
    /// </summary>
    [DataField, AutoNetworkedField]
    public int Complexity = 1;

    /// <summary>
    ///     energy using
    /// </summary>
    [DataField, AutoNetworkedField]
    public float EnergyUsing = 0;

    /// <summary>
    /// The components to add when activated.
    /// </summary>
    [DataField(required: true)]
    public Dictionary<string, ComponentRegistry> Components = new();

    /// <summary>
    /// The components to remove when deactivated.
    /// If this is null <see cref="Components"/> is reused.
    /// </summary>
    [DataField]
    public Dictionary<string, ComponentRegistry>? RemoveComponents;
}

public enum ExamineColor
{
    Red,
    Yellow,
    Green
}
