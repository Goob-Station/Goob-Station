// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Trauma.Shared.StatusEffects;

[RegisterComponent, NetworkedComponent]
public sealed partial class TemporaryActionGrantEffectComponent : Component
{
    /// <summary>
    /// The actions that the effect has added
    /// </summary>
    [DataField]
    public List<EntityUid> Actions = new ();

    /// <summary>
    /// The actions that the effect will add
    /// </summary>
    [DataField(required: true)]
    public List<EntProtoId> ActionPrototypes;
}
