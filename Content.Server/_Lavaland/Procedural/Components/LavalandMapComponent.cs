using Content.Shared._Lavaland.Procedural.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server._Lavaland.Procedural.Components;

[RegisterComponent]
public sealed partial class LavalandMapComponent : Component
{
    [ViewVariables]
    public EntityUid Outpost;

    [ViewVariables]
    public int Seed;

    [ViewVariables]
    public ProtoId<LavalandMapPrototype>? PrototypeId;
}
