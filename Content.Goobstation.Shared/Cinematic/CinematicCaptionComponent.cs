using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Cinematic;

/// <summary>
/// A caption typed onto the owning players screen.
/// </summary>
[RegisterComponent]
public sealed partial class CinematicCaptionComponent : Component
{
    [DataField(required: true)]
    public LocId Text;

    [DataField]
    public ProtoId<CinematicCaptionStylePrototype> Style = "CinematicCaptionDefault";

    [DataField]
    public float WriteTime = 1f;

    [DataField]
    public bool ShowSubject;
}
