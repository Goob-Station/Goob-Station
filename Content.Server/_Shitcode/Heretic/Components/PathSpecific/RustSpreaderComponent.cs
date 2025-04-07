using Content.Shared.Spreader;
using Robust.Shared.Prototypes;

namespace Content.Server.Heretic.Components.PathSpecific;

[RegisterComponent]
public sealed partial class RustSpreaderComponent : Component
{
    [DataField]
    public ProtoId<EdgeSpreaderPrototype> SpreaderProto = "Rust";

    [DataField]
    public float LookupRange = 0.1f;

    [DataField]
    public EntProtoId TileRune = "TileHereticRustRune";
}
