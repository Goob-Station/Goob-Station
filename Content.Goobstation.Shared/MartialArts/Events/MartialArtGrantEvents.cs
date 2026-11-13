namespace Content.Goobstation.Shared.MartialArts.Events;

[ByRefEvent]
public sealed partial class BeforeMartialArtKnowledgeAdded(
    MartialArtPrototype martialArt)
    : CancellableEntityEventArgs
{
    public MartialArtPrototype MartialArt { get; } = martialArt;
}

/// <summary>
/// For multi-use learning like scarp
/// </summary>
/// <param name="martialArt"></param>
[ByRefEvent]
public sealed partial class BeforeMartialArtLearningKnowledgeAdded(
    MartialArtPrototype martialArt)
    : CancellableEntityEventArgs
{
    public MartialArtPrototype MartialArt { get; } = martialArt;
}

[ByRefEvent]
public sealed partial class BeforeKravMagaAdded() : CancellableEntityEventArgs;


[ByRefEvent]
public partial record struct MartialArtKnowledgeAdded(MartialArtPrototype MartialArt);

[ByRefEvent]
public partial record struct MartialArtKnowledgeRemoved(MartialArtPrototype MartialArt);
