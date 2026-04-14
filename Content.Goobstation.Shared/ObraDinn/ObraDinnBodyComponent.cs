using Content.Shared.Mobs;
using Robust.Shared.GameStates;
using Robust.Shared.Map;

namespace Content.Goobstation.Shared.ObraDinn;

[RegisterComponent, NetworkedComponent,AutoGenerateComponentState]
public sealed partial class ObraDinnBodyComponent : Component
{
    [DataField]
    public float WitnessRange = 4f;
    [DataField]
    public List<ObraDinnWitness> Witnesses = new List<ObraDinnWitness>();
    [DataField,AutoNetworkedField]
    public EntityCoordinates? Location;
    [DataField,AutoNetworkedField]
    public MapId? Map;
}

public struct ObraDinnWitness(EntityUid entity, EntityCoordinates location, string name, MobState mobState)
{
    public readonly EntityUid Uid = entity;
    public readonly EntityCoordinates Loc = location;
    public readonly string Name = name;
    public readonly MobState MobState = mobState;

    public ObraDinnWitness Copy()
    {
        return new ObraDinnWitness(Uid, Loc, Name, MobState);
    }
} // Quote: "Can i be a witness to my own death? I should think so, i was there".
