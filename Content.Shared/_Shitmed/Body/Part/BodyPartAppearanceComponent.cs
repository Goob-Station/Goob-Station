using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Humanoid.Markings;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitmed.Body.Part;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class BodyPartAppearanceComponent : Component
{
    /// <summary>
    ///     HumanoidVisualLayer type for this body part.
    /// </summary>
    [DataField, AutoNetworkedField]
    public HumanoidVisualLayers Type { get; set; }

    /// <summary>
    ///     Relevant markings for this body part that will be applied on attachment.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<HumanoidVisualLayers, List<Marking>> Markings = new();

    /// <summary>
    ///     ID of this custom base layer. Must be a <see cref="HumanoidSpeciesSpriteLayer"/>.
    /// </summary>
    [DataField, AutoNetworkedField]
    public ProtoId<HumanoidSpeciesSpriteLayer>? ID { get; set; }

    /// <summary>
    ///     Color of this custom base layer. Null implies skin colour if the corresponding <see cref="HumanoidSpeciesSpriteLayer"/> is set to match skin.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Color? Color { get; set; }

    /// <summary>
    ///     Color of this custom base eye layer. Null implies eye colour if the corresponding <see cref="HumanoidSpeciesSpriteLayer"/> is set to match skin.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Color? EyeColor { get; set; }
}
