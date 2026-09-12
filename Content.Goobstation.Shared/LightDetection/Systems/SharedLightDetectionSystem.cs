namespace Content.Goobstation.Shared.LightDetection.Systems;

public abstract class SharedLightDetectionSystem : EntitySystem;

/// <summary>
/// Raised when an entity's light level gets updated
/// </summary>
[ByRefEvent]
public record struct LightLevelUpdated(float NewLightLevel, float OldLightLevel);
