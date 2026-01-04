using Content.Shared.EntityEffects;

namespace Content.Goobstation.Server.TouchSpell;

[RegisterComponent]
public sealed partial class TouchSpellComponent : Component
{
    /// <summary>
    ///     What action is this touch spell coming from? If any.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)] public EntityUid? AssociatedAction;

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
