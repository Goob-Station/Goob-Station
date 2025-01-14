using Content.Server.Teleportation.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Utility;

namespace Content.Server.Teleportation.Components;

/// <summary>
/// Creates a map for a pocket dimension on spawn.
/// When activated by alt verb, spawns a portal to this dimension or closes it.
/// </summary>
[RegisterComponent]
[Access(typeof(PocketDimensionSystem))]
public sealed class PocketDimensionComponent : Component
{
    /// <summary>
    /// The portal to enter the dimension if open, or null if closed.
    /// </summary>
    [ViewVariables]
    public EntityUid? EntryPortal;

    /// <summary>
    /// The portal in the pocket dimension. Created when the entry portal is first opened.
    /// </summary>
    [ViewVariables]
    public EntityUid? ExitPortal;

    /// <summary>
    /// The pocket dimension map. Created when the entry portal is first opened.
    /// </summary>
    [ViewVariables]
    public EntityUid? PocketDimensionMap;

    /// <summary>
    /// Path to the pocket dimension's map file
    /// </summary>
    [DataField]
    public ResPath PocketDimensionPath = new ResPath("/Maps/Misc/pocket-dimension.yml");

    /// <summary>
    /// The prototype to spawn for the portal spawned on the pot.
    /// </summary>
    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string EntryPortalPrototype = "PortalRed";

    /// <summary>
    /// The prototype to spawn for the portal spawned in the pocket dimension.
    /// </summary>
    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string ExitPortalPrototype = "PortalBlue";

    [DataField]
    public SoundSpecifier OpenPortalSound = new SoundPathSpecifier("/Audio/Machines/high_tech_confirm.ogg")
    {
        Params = AudioParams.Default.WithVolume(-2f)
    };

    [DataField]
    public SoundSpecifier ClosePortalSound = new SoundPathSpecifier("/Audio/Machines/button.ogg");
}
