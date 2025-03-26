using Content.Shared.Movement.Pulling.Systems;

namespace Content.Shared._Goobstation.MartialArts.Components;

/// <summary>
/// Base component for martial arts that override the normal grab stages.
/// Allows martial arts to start at more advanced grab stages like Hard grabs.
/// </summary>
public abstract partial class GrabStagesOverrideComponent : Component
{
    public GrabStage StartingStage = GrabStage.Hard;
}
