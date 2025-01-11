using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.StatusEffects;

/// <summary>
///     For use as a status effect. Spawns spider eggs that will hatch into spiders.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SpawnSpiderEggsComponent : SpawnEntityEffectComponent
{
    public override string EntityPrototype { get; set; } = "EggSpiderFertilized";
}
