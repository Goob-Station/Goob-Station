using Content.Goobstation.Shared.Shadowling.Components;
using Content.Goobstation.Shared.Shadowling.Components.Abilities.PreAscension;
using Content.Server.Actions;
using Content.Shared.Actions;
using Content.Shared.Alert;

namespace Content.Goobstation.Server.Shadowling.Systems;

/// <summary>
/// This handles logic for Lesser Shadowlings.
/// Just upgrades the guise ability into Shadow Walk
/// </summary>
public sealed class LesserShadowlingSystem : EntitySystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;
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
        if (!TryComp<ActionsComponent>(uid, out var actions))
            return;

        AddLesserActions(uid, component, thrall, actions);
    }

    private void AddLesserActions(EntityUid uid, LesserShadowlingComponent component, ThrallComponent thrall, ActionsComponent comp)
    {
        _actions.RemoveAction(thrall.ActionGuiseEntity);

        _actions.AddAction(uid, ref component.ShadowWalkAction, component.ShadowWalkActionId, component: comp);
        EnsureComp<ShadowlingShadowWalkComponent>(uid);

        EnsureComp<LightDetectionComponent>(uid);
        var lightMod = EnsureComp<LightDetectionDamageComponent>(uid);

        lightMod.DetectionValueMax = 10;
    }

    private void OnRemove(EntityUid uid, LesserShadowlingComponent component, ComponentRemove args)
    {
        if (!TryComp(uid, out ActionsComponent? comp))
            return;

        _actions.RemoveAction(uid, component.ShadowWalkAction, comp);
        RemComp<ShadowlingShadowWalkComponent>(uid);
        RemComp<LightDetectionComponent>(uid);
        RemComp<LightDetectionDamageComponent>(uid);

        _alerts.ClearAlert(uid, component.AlertProto);
    }
}
