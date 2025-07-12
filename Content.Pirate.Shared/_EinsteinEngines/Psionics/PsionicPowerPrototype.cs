using Content.Shared.Abilities.Psionics;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;

namespace Content.Shared.Psionics;

[Prototype]
public sealed partial class PsionicPowerPrototype : IPrototype
{
    /// <summary>
    ///     The ID of the psionic power to use.
    /// </summary>
    [IdDataField]
    public string ID { get; } = default!;

    /// <summary>
    ///     The name of the psionic power.
    /// </summary>
    [DataField(required: true)]
    public string Name = default!;

    /// <summary>
    ///     What category of psionics does this power come from.
    ///     EG: Mentalics, Anomalists, Blood Cults, Heretics, etc.
    /// </summary>
    [DataField]
    public List<string> PowerCategories = new();

    /// <summary>
    ///     These functions are called when a Psionic Power is added to a Psion.
    /// </summary>
    [DataField(serverOnly: true)]
    public PsionicPowerFunction[] InitializeFunctions { get; private set; } = Array.Empty<PsionicPowerFunction>();

    /// <summary>
    ///     These functions are called when a Psionic Power is removed from a Psion,
    ///     as a rule of thumb these should do the exact opposite of most of a power's init functions.
    /// </summary>
    [DataField(serverOnly: true)]
    public PsionicPowerFunction[] RemovalFunctions { get; private set; } = Array.Empty<PsionicPowerFunction>();

    /// <summary>
    ///     How many "Power Slots" this power occupies.
    /// </summary>
    [DataField]
    public int PowerSlotCost = 1;
}

/// This serves as a hook for psionic powers to modify the psionic component.
[ImplicitDataDefinitionForInheritors]
public abstract partial class PsionicPowerFunction
{
    public abstract void OnAddPsionic(
        EntityUid mob,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager,
        ISharedPlayerManager playerManager,
        ILocalizationManager loc,
        PsionicComponent psionicComponent,
        PsionicPowerPrototype proto);
}
