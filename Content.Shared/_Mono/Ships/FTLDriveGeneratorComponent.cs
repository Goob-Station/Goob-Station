using Content.Shared._NF.Shuttles;

namespace Content.Shared._Mono.Ships;

/// <summary>
/// A component that enhances a shuttle's FTL range.
/// </summary>
[RegisterComponent]
public sealed partial class FTLDriveGeneratorComponent : Component
{
    [ViewVariables]
    public bool Powered;

    /// <summary>
    /// Better engines should have higher priority to not conflict with smaller ones on board.
    /// </summary>
    [DataField]
    public int Priority;

    [DataField]
    public FTLDriveData Data;
}
