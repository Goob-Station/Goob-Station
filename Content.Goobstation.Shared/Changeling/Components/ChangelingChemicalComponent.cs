using Content.Goobstation.Shared.InternalResources.Data;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Changeling.Components;

/// <summary>
/// Component used for handling changeling chemical reserves.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ChangelingChemicalComponent : Component
{
    /// <summary>
    /// The internal resource prototype to be added.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string ResourceProto = "ChangelingChemicals";

    /// <summary>
    /// The InternalResourcesData of the prototype.
    /// </summary>
    [DataField]
    public InternalResourcesData? ResourceData;

    /// <summary>
    /// The multiplier applied to passive chemical generation while on fire.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float FireModifier = 0.25f;

    [DataField]
    public LocId RejuvenatePopup = "changeling-rejuvenate";
}
