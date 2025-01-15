using Content.Shared.FixedPoint;

namespace Content.Shared._Goobstation.PrisonerId;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class PrisonerIdComponent : Component
{

    [DataField]
    public bool BegunSentence = false;

    [DataField]
    public FixedPoint2 SentenceTime = 0;
}
