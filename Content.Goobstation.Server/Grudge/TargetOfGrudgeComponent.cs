namespace Content.Goobstation.Server.Grudge;

/// <summary>
/// This is used for handling the targets of a grudge
/// </summary>
[RegisterComponent]
public sealed partial class TargetOfGrudgeComponent : Component
{
    public Entity<BookOfGrudgesComponent> Book;
}
