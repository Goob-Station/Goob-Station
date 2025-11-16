using Content.Goobstation.Shared.LightDetection.Components;
using Content.Goobstation.Shared.Shadowling.Components.Abilities.Thrall;
using Content.Shared.Actions;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Shadowling.Systems.Abilities.Thrall;

/// <summary>
/// This handles the Guise ability logic.
/// Guise makes you become invisible only in the dark.
/// That doesn't mean it doesn't work on light, however.
/// </summary>
public sealed class ThrallGuiseSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedStealthSystem _stealth = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ThrallGuiseComponent, GuiseEvent>(OnGuise);
        SubscribeLocalEvent<ThrallGuiseComponent, MapInitEvent>(OnStartup);
        SubscribeLocalEvent<ThrallGuiseComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<ThrallGuiseComponent> ent, ref MapInitEvent args)
        => _actions.AddAction(ent.Owner, ref ent.Comp.ActionEnt, ent.Comp.ActionId);

    private void OnShutdown(Entity<ThrallGuiseComponent> ent, ref ComponentShutdown args)
        => _actions.RemoveAction(ent.Owner, ent.Comp.ActionEnt);

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var query = EntityQueryEnumerator<ThrallGuiseComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.Active)
                continue;

            if (TryComp<LightDetectionComponent>(uid, out var lightDetection))
            {
                if (TryComp<StealthComponent>(uid, out var stealth))
                {
                    if (lightDetection.OnLight)
                    {
                        _stealth.SetVisibility(uid, 0.5f, stealth);
                    }
                    else
                    {
                        _stealth.SetVisibility(uid, -1f, stealth);
                    }
                }
            }

            if (_timing.CurTime >= comp.NextUpdate)
            {
                comp.Active = false;
                if (TryComp<StealthComponent>(uid, out var stealth))
                {
                    _stealth.SetVisibility(uid, 1f, stealth);
                    RemComp<LightDetectionComponent>(uid);
                    RemComp<StealthComponent>(uid);
                }
            }
        }
    }

    private void OnGuise(EntityUid uid, ThrallGuiseComponent component, GuiseEvent args)
    {
        if (args.Handled)
            return;

        EnsureComp<LightDetectionComponent>(uid);
        component.NextUpdate = component.GuiseDuration + _timing.CurTime;
        component.Active = true;

        var stealth = EnsureComp<StealthComponent>(uid);
        _stealth.SetVisibility(uid, -1f, stealth);
        args.Handled = true;
    }
}
