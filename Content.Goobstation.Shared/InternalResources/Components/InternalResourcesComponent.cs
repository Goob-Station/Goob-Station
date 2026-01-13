using Content.Goobstation.Shared.InternalResources.Data;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;
using System.Diagnostics.CodeAnalysis;

namespace Content.Goobstation.Shared.InternalResources.Components;

/// <summary>
/// Component that uses for generic internal resources like mana or changeling's chemicals
/// </summary>

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class InternalResourcesComponent : Component
{
    /// <summary>
    /// List of internal resources data that entity have
    /// </summary>
    [ViewVariables]
    [AutoNetworkedField]
    public List<InternalResourcesData> CurrentInternalResources = new();

    public bool HasResourceData(string protoId, [NotNullWhen(true)] out InternalResourcesData? data)
    {
        data = null;

        foreach (var type in CurrentInternalResources)
        {
            if (type.InternalResourcesType == protoId)
            {
                data = type;
                return true;
            }
        }

        return false;
    }
}
