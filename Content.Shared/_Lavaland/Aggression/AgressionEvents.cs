// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Lavaland.Aggression;

/// <summary>
/// Raised on the entity with AggressiveComponent when it added new aggressor.
/// </summary>
public sealed class AggressorAddedEvent(EntityUid aggressor) : EntityEventArgs
{
    [DataField]
    public EntityUid Aggressor = aggressor;
}

/// <summary>
/// Raised on the entity with AggressiveComponent when it removed one of it's aggressors.
/// </summary>
public sealed class AggressorRemovedEvent(EntityUid aggressor) : EntityEventArgs
{
    [DataField]
    public EntityUid Aggressor = aggressor;
}

/// <summary>
/// Raised on the aggressor when a new aggressive is added to it.
/// </summary>
public sealed class AggressiveAddedEvent(EntityUid aggressive) : EntityEventArgs
{
    [DataField]
    public EntityUid Aggressive = aggressive;
}

/// <summary>
/// Raised on the aggressor when the last aggressive entity is being removed and the component is about to get deleted.
/// </summary>
public sealed class AggressiveRemovedEvent(EntityUid aggressive) : EntityEventArgs
{
    [DataField]
    public EntityUid Aggressive = aggressive;
}
