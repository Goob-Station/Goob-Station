// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Robust.Shared.Audio;

namespace Content.Trauma.Shared.Bouncing;

[RegisterComponent, NetworkedComponent]
public sealed partial class BounceableComponent : Component
{
    /// <summary>
    /// How many times has this entity bounced off another one.
    /// </summary>
    [DataField]
    public int TimesBounced;

    /// <summary>
    /// How many times it needs to bounce to apply effects.
    /// </summary>
    [DataField]
    public int BouncesRequired = 9;

    /// <summary>
    /// Time to add to NextValidBounceTime when entity collides.
    /// </summary>
    [DataField]
    public TimeSpan GraceTime = TimeSpan.FromSeconds(2);

    /// <summary>
    /// The effects to apply when collided.
    /// </summary>
    [DataField]
    public EntityEffect[] Effects;

    /// <summary>
    /// The entity to spawn when bounced enough.
    /// </summary>
    [DataField]
    public EntProtoId EntityToSpawn;

    /// <summary>
    /// When should we start counting collisions as bounces again.
    /// </summary>
    [DataField]
    public TimeSpan NextValidBounceTime = TimeSpan.Zero;

    [DataField]
    public SoundPathSpecifier BounceSound =  new SoundPathSpecifier("/Audio/Effects/Footsteps/bounce.ogg");
}
