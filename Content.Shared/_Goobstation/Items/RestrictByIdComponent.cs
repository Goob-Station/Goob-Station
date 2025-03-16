using Content.Shared.Access;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Items;


/// <summary>
///     Component used to restrict the usage of an item, by the state of the wielders ID.
/// </summary>
[RegisterComponent]
public sealed partial class RestrictByIdComponent : Component
{
    /// <summary>
    ///     Which accesses to restrict the item to.
    /// </summary>
    [DataField("access")]
    public HashSet<ProtoId<AccessLevelPrototype>> AccessLists = new();

    /// <summary>
    ///     Whether the restriction should be inverted
    /// </summary>
    /// <remarks>
    /// For example, setting the ID to "Clown" and invert to true, would allow anyone but someone with a clown ID to use it.
    /// </remarks>
    [DataField]
    public bool Invert { get; set; } = false;

    /// <summary>
    ///     Whether melee attacks should be restricted. True by default.
    /// </summary>
    [DataField]
    public bool RestrictMelee { get; set; } = true;

    /// <summary>
    ///     Whether ranged attacks should be restricted. True by default.
    /// </summary>
    [DataField]
    public bool RestrictRanged { get; set; } = true;
}
