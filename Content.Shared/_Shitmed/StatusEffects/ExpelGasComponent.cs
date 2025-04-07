using Content.Shared.Atmos;
using Robust.Shared.GameStates;
namespace Content.Shared._Shitmed.StatusEffects;

/// <summary>
///     Randomly spawns gas of a given type.
/// </summary>
[RegisterComponent]
public sealed partial class ExpelGasComponent : Component
{
    public List<Gas> PossibleGases = new()
    {
        Gas.Oxygen,
        Gas.Plasma,
        Gas.Nitrogen,
        Gas.CarbonDioxide,
        Gas.Tritium,
        Gas.Ammonia,
        Gas.NitrousOxide,
        Gas.Frezon,
        Gas.BZ, ///tg/ gases
        Gas.Healium, ///tg/ gases
        Gas.Nitrium, ///tg/ gases
    };
}
