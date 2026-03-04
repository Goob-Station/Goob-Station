// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Trauma.Shared.ShadowDemon;

/// <summary>
/// Gives you shadow crawl action.
/// Allows an entity to use ShadowCrawl action.
/// Shadow Crawl is an ability that lets you jaunt in darkness,
/// and can reduce the damage you take from lights.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class ShadowCrawlComponent : Component
{
    /// <summary>
    /// The phase in effect when you start the jaunt
    /// </summary>
    [DataField]
    public EntProtoId PhaseIn;

    /// <summary>
    /// The phase out effect when you start the jaunt
    /// </summary>
    [DataField]
    public EntProtoId PhaseOut;

    /// <summary>
    /// How much damage to reduce from lights while in jaunt
    /// </summary>
    [DataField]
    public float DamageModiferFromLights = 0.5f;

    /// <summary>
    /// The cooldown applied to the action when exiting the jaunt
    /// </summary>
    [DataField]
    public TimeSpan ActionCooldown = TimeSpan.FromSeconds(4);

    /// <summary>
    /// The cooldown applied to the action when shooting a grapple
    /// </summary>
    [DataField]
    public TimeSpan ActionCooldownAfterGrapple = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Whether we are in jaunt or not
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Active;

    [DataField]
    public EntProtoId ActionId = "ShadowCrawlAction";

    [DataField, AutoNetworkedField]
    public EntityUid? ActionUid;
}
