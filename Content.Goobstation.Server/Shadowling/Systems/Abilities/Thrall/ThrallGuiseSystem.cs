using Content.Server.Actions;
using Content.Server.Stealth;
using Content.Shared._EE.Shadowling;
using Content.Shared._EE.Shadowling.Components;
using Content.Shared._EE.Shadowling.Thrall;
using Content.Shared.Stealth.Components;
using Robust.Shared.Timing;


namespace Content.Server._EE.Shadowling.Thrall;


/// <summary>
/// This handles the Guise ability logic.
/// Guise makes you become invisible only in the dark.
/// That doesn't mean it doesn't work on light, however.
/// </summary>
public sealed class ThrallGuiseSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly StealthSystem _stealth = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ThrallGuiseComponent, GuiseEvent>(OnGuise);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var query = EntityQueryEnumerator<ThrallGuiseComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.Active)
            {
                if (TryComp<LightDetectionComponent>(uid, out var lightDetection))
                {
                    if (TryComp<StealthComponent>(uid, out var stealth))
                    {
                        if (lightDetection.IsOnLight)
                        {
                            _stealth.SetVisibility(uid, 0.5f, stealth);
                        }
                        else
                        {
                            _stealth.SetVisibility(uid, -1f, stealth);
                        }
                    }
                }
                // Start timer
                comp.Timer -= frameTime;

                if (comp.Timer <= 0)
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
    }

    private void OnGuise(EntityUid uid, ThrallGuiseComponent component, GuiseEvent args)
    {
        EnsureComp<LightDetectionComponent>(uid);
        component.Timer = component.GuiseDuration;
        component.Active = true;

        var stealth = EnsureComp<StealthComponent>(uid);
        _stealth.SetVisibility(uid, -1f, stealth);
        _actions.StartUseDelay(args.Action);
    }
}
