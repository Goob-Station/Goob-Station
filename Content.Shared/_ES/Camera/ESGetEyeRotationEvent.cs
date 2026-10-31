using Content.Shared.Camera;

namespace Content.Shared._ES.Camera;

/// <summary>
///     Raised directed by-ref on the local eye entity whenever its lerped rotation is applied.
///     Should be subscribed to by any systems that want to modify an entity's eye rotation,
///     so that they do not override each other.
/// </summary>
/// <remarks>
///     Counterpart of <see cref="GetEyeOffsetEvent"/>, but for rotation, to use for screenshake.
/// </remarks>
[ByRefEvent]
public record struct ESGetEyeRotationEvent(Angle Rotation);
