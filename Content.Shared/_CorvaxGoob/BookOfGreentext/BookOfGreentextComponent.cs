using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._CorvaxGoob.BookOfGreentext;

[RegisterComponent]
public sealed partial class BookOfGreentextComponent : Component
{
    /// <summary>
    /// Delay before the book will be linked with user
    /// </summary>
    [DataField]
    public TimeSpan UseDelay = TimeSpan.FromSeconds(3);
}

[Serializable, NetSerializable]
public sealed partial class BookOfGreentextDoAfterEvent : SimpleDoAfterEvent
{
}
