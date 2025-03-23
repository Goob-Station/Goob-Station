namespace Content.Shared._Goobstation.CoversBothEars;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class CoversBothEarsComponent : Component
{
    [ViewVariables]
    public EntityUid VirtualEnt = EntityUid.Invalid;

    public const string EarsSlot = "ears";

    public const string Ears2Slot = "ears2";
}
