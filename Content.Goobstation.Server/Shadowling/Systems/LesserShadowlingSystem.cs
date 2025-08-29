using Content.Goobstation.Shared.Shadowling.Components;
using Content.Goobstation.Shared.Shadowling.Components.Abilities.PreAscension;
using Content.Goobstation.Shared.Shadowling.Components.Abilities.Thrall;
using Content.Shared.Alert;

namespace Content.Goobstation.Server.Shadowling.Systems;

/// <summary>
/// This handles logic for Lesser Shadowlings.
/// Just upgrades the guise ability into Shadow Walk
/// </summary>
public sealed class LesserShadowlingSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LesserShadowlingComponent, ComponentRemove>(OnRemove);
        SubscribeLocalEvent<LesserShadowlingComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, LesserShadowlingComponent component, ComponentStartup args)
    {
        if (!TryComp<ThrallComponent>(uid, out var thrall))
            return;

        AddLesserActions(uid);
    }

    private void AddLesserActions(EntityUid uid)
    {
        RemComp<ThrallGuiseComponent>(uid);

    }

    private void OnRemove(EntityUid uid, LesserShadowlingComponent component, ComponentRemove args)
    {
        RemComp<ShadowlingShadowWalkComponent>(uid);
        RemComp<LightDetectionComponent>(uid);
        RemComp<LightDetectionDamageComponent>(uid);
    }
}
