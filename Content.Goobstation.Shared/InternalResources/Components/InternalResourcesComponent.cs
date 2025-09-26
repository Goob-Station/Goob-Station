using Content.Goobstation.Shared.InternalResources.Data;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Goobstation.Shared.InternalResources.Components;

/// <summary>
/// Component that uses for generic internal resources like mana or changeling's chemicals
/// </summary>

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class InternalResourcesComponent : Component
{
    /// <summary>
    /// List of prototypes for internal resources initialization
    /// </summary>
    [DataField(customTypeSerializer: typeof(PrototypeIdListSerializer<InternalResourcesPrototype>))]
    public List<string> InternalResourcesTypes;

    /// <summary>
    /// List of internal resources data that entity have
    /// </summary>
    [ViewVariables]
    [AutoNetworkedField]
    public List<InternalResourcesData> CurrentInternalResources = new();
}
