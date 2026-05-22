// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.EntityEffects;
using Content.Shared.Whitelist;

namespace Content.Trauma.Shared.EffectOnEaten;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class EntityEffectOnEatenComponent : Component
{
    /// <summary>
    /// The effects to apply.
    /// </summary>
    [DataField]
    public EntityEffect[] Effects;

    /// <summary>
    /// Optional scale multiplier for the effects.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Scale = 1f;

    /// <summary>
    /// If the entity whitelist is not null only entities are contained in the EntProtoId will have the effects added to them
    /// </summary>
    [DataField]
    public List<EntProtoId>? EntityWhitelist;

    /// <summary>
    /// If the whitelist is not null then the entity will be rejected if it does not pass the whitelist
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist;
}
