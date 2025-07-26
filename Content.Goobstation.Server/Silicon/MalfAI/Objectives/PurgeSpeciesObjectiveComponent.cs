using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Silicon.MalfAI.Objectives;

[RegisterComponent]
public sealed partial class PurgeSpeciesObjectiveComponent : Component
{
    [DataField]
    public string TitleLoc = string.Empty;

    [DataField]
    public ProtoId<SpeciesPrototype> TargetSpeciesPrototype;

    [DataField(required: true)]
    public List<ProtoId<SpeciesPrototype>> SpeciesWhitelist;
}