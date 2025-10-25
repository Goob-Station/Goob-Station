using Content.Shared.Alert;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.InternalResources.Data;

/// <summary>
/// Prototype for internal resources type. Mostly contain visualization and information data.
/// </summary>
[Prototype]
public sealed class InternalResourcesPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public LocId Name;

    [DataField]
    public LocId? Description;

    /// <summary>
    /// Alert prototype for inner resources visualising
    /// </summary>
    [DataField("alert")]
    public ProtoId<AlertPrototype> AlertPrototype = "ChangelingChemicals";

    /// <summary>
    /// Base resources regeneration rate per update time
    /// </summary>
    [DataField("regenerationRate")]
    public float BaseRegenerationRate = 1f;

    /// <summary>
    /// Base resources maximum amount
    /// </summary>
    [DataField("maxAmount")]
    public float BaseMaxAmount = 100f;

    /// <summary>
    /// Base amount of resources when these internal resources is added to entity
    /// </summary>
    [DataField("startingAmount")]
    public float BaseStartingAmount = 100f;
}
