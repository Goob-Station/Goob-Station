namespace Content.Goobstation.Shared.ActionTargetMarkSystem;

public interface IActionTargetMarkSystem : IEntitySystem
{
    EntityUid? Target { get; }
    EntityUid? Mark { get; }
    void SetMark(EntityUid? uid);
}

