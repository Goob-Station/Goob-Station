using Content.Shared.EntityEffects;
using Content.Shared.StatusIcon;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;

namespace Content.Goobstation.Shared.Wraith.Curses;

[Prototype]
public sealed class CursePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = default!;

    [DataField]
    public string Name { get; } = string.Empty;

    /// <summary>
    /// A dictionary that holds the random value that determines which list of entity effects will happen on the user.
    /// For example -> There's a 0.5 chance to roll a PopupEntityEffect and a VomitPopupEntityEffect together
    /// The (internal) probability of all EntityEffects should be 1.0 (when defined)
    /// </summary>
    [DataField(required: true, serverOnly: true)]
    public Dictionary<float, List<EntityEffect>> Effects = new();

    /// <summary>
    /// Components added to the entity when this curse gets added
    /// </summary>
    [DataField(serverOnly: true)]
    public ComponentRegistry? Components = new();

    /// <summary>
    /// How often to update the curse in seconds
    /// </summary>
    [DataField(required: true)]
    public float Update;

    [DataField]
    public ProtoId<CurseStatusIconPrototype>? StatusIcon;
}

[Prototype]
public sealed partial class CurseStatusIconPrototype : StatusIconPrototype, IInheritingPrototype
{
    /// <inheritdoc />
    [ParentDataField(typeof(AbstractPrototypeIdArraySerializer<CurseStatusIconPrototype>))]
    public string[]? Parents { get; private set; }

    /// <inheritdoc />
    [NeverPushInheritance]
    [AbstractDataField]
    public bool Abstract { get; private set; }
}
