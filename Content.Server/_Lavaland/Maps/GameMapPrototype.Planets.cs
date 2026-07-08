using Content.Shared._Lavaland.Procedural.Prototypes;
using Robust.Shared.Prototypes;

// ReSharper disable once CheckNamespace
namespace Content.Server.Maps;

public sealed partial class GameMapPrototype
{
    /// <summary>
    /// Contains info about planets that we have to spawn assigned from this game map.
    /// </summary>
    [DataField]
    public List<ProtoId<LavalandMapPrototype>> Planets = new() { "Lavaland" };
}
