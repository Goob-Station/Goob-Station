using Content.Shared.Nutrition.Components;
using Robust.Shared.GameStates;

namespace Content.Shared.Kitchen.Components;

/// <summary>
///     Applies to items that are capable of butchering entities, or
///     are otherwise sharp for some purpose.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class SharpComponent : Component
{
    /// <summary>
    /// List of the entities that are currently being butchered.
    /// </summary>
    // TODO just make this a tool type. Move SharpSystem to shared.
    [AutoNetworkedField]
    public readonly HashSet<EntityUid> Butchering = [];

    /// <summary>
    /// Affects butcher delay of the <see cref="ButcherableComponent"/>.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ButcherDelayModifier = 1.0f;

    /// <summary>
    /// Shitmed: Whether this item had <c>SurgeryToolComponent</c> before sharp was added.
    /// </summary>
    [DataField]
    public bool HadSurgeryTool;

    /// <summary>
    /// Shitmed: Whether this item had <c>ScalpelComponent</c> before sharp was added.
    /// </summary>
    [DataField]
    public bool HadScalpel;

    /// <summary>
    /// Shitmed: Whether this item had <c>BoneSawComponent</c> before sharp was added.
    /// </summary>
    [DataField]
    public bool HadBoneSaw;
}
