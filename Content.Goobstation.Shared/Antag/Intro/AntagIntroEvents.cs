using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Antag.Intro;

/// <summary>
/// Tells the client to play the intro.
/// </summary>
[Serializable, NetSerializable]
public sealed class AntagIntroPlayEvent(ProtoId<AntagIntroPrototype> intro) : EntityEventArgs
{
    public readonly ProtoId<AntagIntroPrototype> Intro = intro;
}

/// <summary>
/// Tells the server when the intro has finished.
/// </summary>
[Serializable, NetSerializable]
public sealed class AntagIntroFinishedEvent(ProtoId<AntagIntroPrototype> intro) : EntityEventArgs
{
    public readonly ProtoId<AntagIntroPrototype> Intro = intro;
}
