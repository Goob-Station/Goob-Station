using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna;

/// <summary>
/// Arguments that are used for Megafauna Actions and Conditions.
/// </summary>
public record struct MegafaunaCalculationBaseArgs(
    EntityUid BossEntity,
    IEntityManager EntityManager,
    IPrototypeManager PrototypeMan,
    System.Random Random);
