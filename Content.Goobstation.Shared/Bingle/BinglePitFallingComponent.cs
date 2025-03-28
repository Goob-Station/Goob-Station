using System;
using System.Numerics;
using Content.Goobstation.Common.Bingle;
using Robust.Shared.Analyzers;
using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.Bingle;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
public sealed partial class BinglePitFallingComponent : Component
{
    /// <summary>
    ///     Time it should take for the falling animation (scaling down) to complete.
    /// </summary>
    [DataField("animationTime")]
    public TimeSpan AnimationTime = TimeSpan.FromSeconds(1.5f);

    /// <summary>
    ///     Time it should take in seconds for the entity to actually delete
    /// </summary>
    [DataField("deletionTime")]
    public TimeSpan DeletionTime = TimeSpan.FromSeconds(1.8f);

    [DataField("nextDeletionTime", customTypeSerializer:typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan NextDeletionTime = TimeSpan.Zero;

    /// <summary>
    ///     Original scale of the object so it can be restored if the component is removed in the middle of the animation
    /// </summary>
    public Vector2 OriginalScale = Vector2.Zero;

    /// <summary>
    ///     Scale that the animation should bring entities to.
    /// </summary>
    public Vector2 AnimationScale = new Vector2(0.01f, 0.01f);

    /// <summary>
    ///     the pit your about to fall into
    /// </summary>
    public BinglePitComponent Pit = new BinglePitComponent();
}
