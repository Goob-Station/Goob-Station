using Content.Shared.Body.Prototypes;
using Content.Shared.Body.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Content.Shared.Body.Organ;
using Content.Shared.Body.Part;
using Content.Shared._Shitmed.Body.Part;

namespace Content.Shared.Body.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedBodySystem))]
public sealed partial class BodyComponent : Component
{
    public HashSet<OrganComponent> OriginalOrgans = new();
    public HashSet<BodyPartComponent> OriginalBodyParts = new();
    public HashSet<BodyPartAppearanceComponent> OriginalAppearances = new();

    /// <summary>
    /// Relevant template to spawn for this body.
    /// </summary>
    [DataField, AutoNetworkedField]
    public ProtoId<BodyPrototype>? Prototype;

    /// <summary>
    /// Container that holds the root body part.
    /// </summary>
    /// <remarks>
    /// Typically is the torso.
    /// </remarks>
    [ViewVariables] public ContainerSlot RootContainer = default!;

    [ViewVariables]
    public string RootPartSlot => RootContainer.ID;

    [DataField, AutoNetworkedField]
    public SoundSpecifier GibSound = new SoundCollectionSpecifier("gib");

    /// <summary>
    /// The amount of legs required to move at full speed.
    /// If 0, then legs do not impact speed.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int RequiredLegs;

    [ViewVariables]
    [DataField, AutoNetworkedField]
    public HashSet<EntityUid> LegEntities = new();
}
