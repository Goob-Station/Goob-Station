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
    /// entity White list
    /// </summary>
    [DataField]
    public List<EntProtoId>? EntityWhiteList;

    /// <summary>
    /// White list
    /// </summary>
    [DataField]
    public EntityWhitelist? WhiteList;
}
