// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Random;
using Content.Shared.EntityTable.EntitySelectors;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.RatKing.Components;

/// <summary>
/// This is used for entities that can be
/// rummaged through by the rat king to get loot.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class RummageableComponent : Component
{
    /// <summary>
    /// Whether or not this entity has been rummaged through already.
    /// </summary>
    [DataField("looted"), ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public bool Looted;

    /// <summary>
    /// DeltaV: Last time the object was looted, used to check if cooldown has expired
    /// </summary>
    [ViewVariables]
    public TimeSpan? LastLooted;

    /// <summary>
    /// DeltaV: Minimum time between rummage attempts
    /// </summary>
    [DataField("rummageCooldown"), ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public TimeSpan RummageCooldown = TimeSpan.FromMinutes(1);

    /// <summary>
    /// How long it takes to rummage through a rummageable container.
    /// </summary>
    [DataField("rummageDuration"), ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public float RummageDuration = 2f;

    /// <summary>
    /// The entity table to select loot from.
    /// </summary>
    [DataField(required: true)]
    public EntityTableSelector Table = default!;

    /// <summary>
    /// Sound played on rummage completion.
    /// </summary>
    [DataField("sound")]
    public SoundSpecifier? Sound = new SoundCollectionSpecifier("storageRustle");
}
