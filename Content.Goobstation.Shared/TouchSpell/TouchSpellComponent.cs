using Content.Shared.EntityEffects;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.TouchSpell;

[RegisterComponent, NetworkedComponent]
public sealed partial class TouchSpellComponent : Component
{
    /// <summary>
    ///     What action is this touch spell coming from? If any.
    /// </summary>
    [DataField] public EntityUid? AssociatedAction;

    [DataField] public EntityUid? AssociatedPerformer;

    /// <summary>
    ///     Should it get deleted after use?
    ///     This is supposed to be passed to these entity effects that it's supposed to cause.
    /// </summary>
    [DataField] public bool QueueDelete = false;

    /// <summary>
    ///     What effects should it apply on touch?
    /// </summary>
    [DataField] public List<EntityEffect> Effects = new();
}
