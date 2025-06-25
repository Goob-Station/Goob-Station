using Content.Server.Actions;
using Content.Shared._EE.Shadowling;
using Content.Shared._EE.Shadowling.Components;
using Content.Shared.Actions;
using Content.Shared.Alert;


namespace Content.Server._EE.Shadowling;


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

        _actions.AddAction(uid, ref component.ShadowWalkActionId, component.ShadowWalkAction, component: comp);
        EnsureComp<ShadowlingShadowWalkComponent>(uid);

        EnsureComp<LightDetectionComponent>(uid);
        var lightMod = EnsureComp<LightDetectionDamageModifierComponent>(uid);

        lightMod.DetectionValueMax = 10;
    }

    private void OnRemove(EntityUid uid, LesserShadowlingComponent component, ComponentRemove args)
    {
        if (!TryComp(uid, out ActionsComponent? comp))
            return;

        _actions.RemoveAction(uid, component.ShadowWalkActionId, comp);
        RemComp<ShadowlingShadowWalkComponent>(uid);
        RemComp<LightDetectionComponent>(uid);
        RemComp<LightDetectionDamageModifierComponent>(uid);

        _alerts.ClearAlert(uid, component.AlertProto);
    }
}
