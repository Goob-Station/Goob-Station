using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Implants.AddComponentsImplant;

/// <summary>
///     When added to an implanter will add the passed in components to the implanted entity.
/// </summary>
/// <remarks>
///     Warning: Multiple implants with this component adding the same components will not properly remove components
///     unless removed in the inverse order of their injection (Last in, first out).
/// </remarks>
[RegisterComponent]
public sealed partial class AddComponentsImplantComponent : Component
{
    /// <summary>
    ///     What components will be added to the entity. If the component already exists, it will be skipped.
    /// </summary>
    [DataField(required: true)]
    public ComponentRegistry ComponentsToAdd = new();

    /// <summary>
    ///     What components were added to the entity after implanted. Is used to know what components to remove.
    /// </summary>
    [DataField]
    public ComponentRegistry AddedComponents = new();
}
