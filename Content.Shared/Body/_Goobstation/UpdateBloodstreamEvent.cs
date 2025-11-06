using Content.Shared.Body.Components;
using Content.Shared.Chemistry.Components;

namespace Content.Shared.Body._Goobstation;

/// <summary>
/// [WOUNDMED] Event for updating the bloodstream of an entity.
/// </summary>
/// <param name="Entity">The entity to update.</param>
/// <param name="Bloodstream">The bloodstream component to update.</param>
/// <param name="AnythingToDo">Whether there is anything to do to the bloodstream.</param>
/// <param name="BleedAmount">The amount of blood to bleed.</param>
/// </summary>
[ByRefEvent]
public struct UpdateBloodstreamEvent(EntityUid entity, ref BloodstreamComponent bloodstream)
{
    public readonly EntityUid Entity = entity;
    public BloodstreamComponent Bloodstream = bloodstream;
    public bool AnythingToDo { get; set; } = false;
    public float BleedAmount { get; set; } = 0f;
}

/// <summary>
/// [WOUNDMED] Event for updating the blood freshness of an entity.
/// </summary>
/// <param name="TempSolution">The temporary solution to update.</param>
[ByRefEvent]
public struct UpdateBloodFreshnessEvent(Solution tempSolution)
{
    public Solution TempSolution = tempSolution;
}
