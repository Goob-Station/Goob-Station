namespace Content.Goobstation.Shared.ActionTargetMarkSystem;

[RegisterComponent]
public sealed partial class ActionTargetMarkComponent : Component
{
    [ViewVariables]
    public EntityUid? Target;

    [ViewVariables]
    public EntityUid? Mark;
}
