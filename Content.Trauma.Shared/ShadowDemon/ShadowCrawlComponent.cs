using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Trauma.Shared.ShadowDemon;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class ShadowCrawlComponent : Component
{
    [DataField]
    public EntProtoId PhaseIn;

    [DataField]
    public  EntProtoId PhaseOut;

    [DataField]
    public float DamageModiferFromLights = 0.5f;

    [DataField]
    public float SpeedBuff = 2;

    [DataField]
    public TimeSpan CastTime = TimeSpan.FromSeconds(2f);

    [ViewVariables, AutoNetworkedField]
    public bool Active;
}

/// <summary>
/// Raised when the crawl gets activated
/// </summary>
[ByRefEvent]
public record struct ShadowCrawlActivatedEvent(float LightDamageModifier = 1f);

/// <summary>
/// Raised when the crawl gets de-activated
/// </summary>
[ByRefEvent]
public record struct ShadowCrawlDeActivatedEvent(float LightDamageModifier = 1f);

/// <summary>
/// Raised when attempting to shadow crawl
/// </summary>
/// <param name="Cancelled"></param> Are we allowed to crawl?
[ByRefEvent]
public record struct ShadowCrawlAttemptEvent(bool Cancelled = false);

public sealed partial class ShadowCrawlEvent : InstantActionEvent;
