using Robust.Shared.GameStates;

namespace Content.Shared._CorvaxGoob.BookOfGreentext;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CurseOfBookOfGreentextComponent : Component
{
    /// <summary>
    /// Status of "objective".
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Completed = true;

    /// <summary>
    /// Book that linked with user.
    /// </summary>
    [DataField]
    public EntityUid? Book = null;

    /// <summary>
    /// Cooldown before next check for contains the book in owner inventory.
    /// </summary>
    [DataField]
    public TimeSpan NextUpdate;
}
