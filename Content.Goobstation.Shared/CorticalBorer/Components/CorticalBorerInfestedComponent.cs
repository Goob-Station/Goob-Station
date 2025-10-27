using Content.Shared._Starlight.CollectiveMind;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.CorticalBorer.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class CorticalBorerInfestedComponent : Component
{
    /// <summary>
    /// Borer in the person
    /// </summary>
    [ViewVariables]
    public Entity<CorticalBorerComponent> Borer = new();

    /// <summary>
    ///     Container for borer
    /// </summary>
    public Container InfestationContainer = new();

    /// <summary>
    /// is the person under the borer's control
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan? ControlTimeEnd;

    [ViewVariables]
    public EntityUid? OriginalMindId;

    [ViewVariables]
    public EntityUid BorerMindId;

    /// <summary>
    /// Where the mind gets hidden when the worm takes control
    /// </summary>
    public Container ControlContainer;

    /// <summary>
    /// Abilities to be removed once host gets control back
    /// </summary>
    public List<EntityUid> RemoveAbilities = new();

    /// <summary>
    /// Reform action that was removed from the host when borer took control.
    /// </summary>
    public EntityUid? RemovedReformAction;

    /// <summary>
    /// Whether to not remove the hivemind on abandoning direct control.
    /// </summary>
    [ViewVariables]
    public bool HadHivemind = false;

    /// <summary>
    /// What default hivemind channel to return after abandoning direct control.
    /// </summary>
    [ViewVariables]
    public ProtoId<CollectiveMindPrototype>? OldDefault = null;
}
