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
    public List<ProtoId<AccessLevelPrototype>> AccessLists = [];

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

    /// <summary>
    ///     Whether the item can be emagged to remove it's access locks.
    /// </summary>
    [DataField]
    public bool IsEmaggable { get; set; } = false;

    /// <summary>
    ///     Whether the item is currently emagged.
    /// </summary>
    [DataField]
    public bool IsEmagged = false;

    /// <summary>
    ///     Whether ranged attacks should be restricted. True by default.
    /// </summary>
    [DataField]
    public string FailText { get; set; } = "restricted-by-id-component-attack-fail-id-wrong";
}
