using Content.Pirate.Shared.Aiming.Events;
using Content.Shared.Alert;
using Content.Shared.Movement.Pulling.Components;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Prototypes;

namespace Content.Pirate.Shared.Aiming;

public sealed partial class SharedOnSightSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<OnSightComponent, MoveEvent>(OnMove);
        SubscribeLocalEvent<OnSightComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<OnSightComponent, OnAimerShootingEvent>(OnAimerShooting);
    }
    private void OnStartup(EntityUid uid, OnSightComponent component, ComponentStartup args)
    {
        if (_proto.TryIndex<AlertPrototype>("OnSightAlert", out var alertProto))
            _alerts.ShowAlert(uid, alertProto);
    }
    private void OnMove(EntityUid uid, OnSightComponent component, ref MoveEvent args)
    {
        if (TryComp<PullableComponent>(uid, out var pullComp) && pullComp.BeingPulled && pullComp.Puller != null)
        {
            foreach (var entity in component.AimedAtBy)
            {
                if (entity == pullComp.Puller.Value)
                    return;
            }
        }
        if (_proto.TryIndex<AlertPrototype>("OnSightAlert", out var alertProto))
            _alerts.ClearAlert(uid, alertProto);
        foreach (var entity in component.AimedAtWith.ToArray())
        {
            var ev = new OnAimingTargetMoveEvent(uid);
            RaiseLocalEvent(entity, ev);
        }
        RemComp<OnSightComponent>(uid);
    }
    public void OnAimerShooting(EntityUid uid, OnSightComponent component, OnAimerShootingEvent args)
    {
        foreach (var entity in component.AimedAtWith.ToArray())
        {
            if (entity == args.Gun)
            {
                component.AimedAtWith.Remove(entity);
                break;
            }
        }
        foreach (var entity in component.AimedAtBy.ToArray())
        {
            if (entity == args.User)
            {
                component.AimedAtBy.Remove(entity);
                break;
            }
        }
        if (component.AimedAtWith.Count == 0 || component.AimedAtBy.Count == 0)
        {
            if (_proto.TryIndex<AlertPrototype>("OnSightAlert", out var alertProto))
                _alerts.ClearAlert(uid, alertProto);
            RemComp<OnSightComponent>(uid);
        }
    }
}
