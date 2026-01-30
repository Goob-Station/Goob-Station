using Content.Goobstation.Shared.InternalResources.Data;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using System.Diagnostics.CodeAnalysis;

namespace Content.Goobstation.Shared.InternalResources.Components;

/// <summary>
/// Component that uses for generic internal resources like mana or changeling's chemicals
/// </summary>

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class InternalResourcesComponent : Component
{
    /// <summary>
    /// List of internal resources data that entity have
    /// </summary>
    [ViewVariables]
    [DataField, AutoNetworkedField]
    public List<InternalResourcesData> CurrentInternalResources = new();

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField, AutoPausedField]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    [DataField]
    public TimeSpan UpdateDelay = TimeSpan.FromSeconds(1);

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
