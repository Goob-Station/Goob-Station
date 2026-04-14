using Content.Shared.Mobs;
using Robust.Shared.GameStates;
using Robust.Shared.Map;

namespace Content.Goobstation.Shared.ObraDinn;

[RegisterComponent, NetworkedComponent,AutoGenerateComponentState]
public sealed partial class ObraDinnBodyComponent : Component
{
    /// <summary>
    /// range of witnesses we store
    /// </summary>
    [DataField]
    public float WitnessRange = 4f;

    /// <summary>
    /// witnesses data stored
    /// </summary>
    [DataField]
    public List<ObraDinnWitness> Witnesses = new List<ObraDinnWitness>();

    /// <summary>
    /// Where it happened
    /// </summary>
    [DataField,AutoNetworkedField]
    public EntityCoordinates? Location;

    /// <summary>
    /// Map where it happened
    /// </summary>
    [DataField,AutoNetworkedField]
    public MapId? Map;
}

/// <summary>
/// data for anyone around a player dying
/// </summary>
/// <param name="entity">uid</param>
/// <param name="location"> location</param>
/// <param name="name"> Indentity name</param>
/// <param name="mobState"> mobstate comp</param>
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
