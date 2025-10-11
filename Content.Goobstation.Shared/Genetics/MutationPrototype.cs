using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Genetics;

/// <summary>
/// A mutation that can be added to entities with <see cref="MutatableComponent"/>.
/// </summary>
[Prototype]
public sealed partial class MutationPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// User-facing name of this mutation.
    /// </summary>
    [ViewVariables]
    public string LocalizedName => Loc.GetString($"mutation-{ID}-name");

    /// <summary>
    /// Popup shown to a player when they mutate.
    /// </summary>
    [ViewVariables]
    public string LocalizedPopup => Loc.GetString($"mutation-{ID}-popup");

    /// <summary>
    /// Instability added to the mutated entity by this mutation.
    /// </summary>
    [DataField(required: true)]
    public int Instability;

    /// <summary>
    /// Permanent mutations can never be removed by any means.
    /// </summary>
    [DataField]
    public bool Permanent;

    /// <summary>
    /// These mutations are required by this one.
    /// </summary>
    [DataField]
    public List<ProtoId<MutationPrototype>> Required = new();

    /// <summary>
    /// This mutation cannot be added if any of these are present.
    /// Only works one-way, in most cases you should mirror them.
    /// </summary>
    [DataField]
    public List<ProtoId<MutationPrototype>> Conflicts = new();

    /// <summary>
    /// Removes these mutations when this one is added.
    /// </summary>
    [DataField]
    public List<ProtoId<MutationPrototype>> Removes = new();

    /// <summary>
    /// Components added to the mutated entity when active.
    /// WARNING: If these already exist before being added the old data is forgotten and will be removed if it gets deactivated.
    /// </summary>
    [DataField]
    public ComponentRegistry? AddedComponents;

    /// <summary>
    /// Components removed from the mutated entity when active.
    /// WARNING: These do not have their data re-added, only the default component!
    /// </summary>
    [DataField]
    public ComponentRegistry? RemovedComponents;
}
