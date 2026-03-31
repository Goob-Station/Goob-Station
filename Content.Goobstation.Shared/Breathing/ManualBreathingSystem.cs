using Content.Shared.Input;
using Robust.Shared.Input.Binding;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Breathing;

public sealed class ManualBreathingSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ManualBreathingComponent, MapInitEvent>(OnMapInit);

        CommandBinds.Builder
            .Bind(ContentKeyFunctions.Breathe,
                InputCmdHandler.FromDelegate(session => OnBreathePressed(session, true),
                    session => OnBreathePressed(session, false), false, false))
            .Register<ManualBreathingSystem>();
    }

    public override void Shutdown()
    {
        base.Shutdown();
        CommandBinds.Unregister<ManualBreathingSystem>();
    }

    private void OnMapInit(Entity<ManualBreathingComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.LastManualBreathTime = _timing.CurTime;
        Dirty(ent);
    }

    private void OnBreathePressed(ICommonSession? session, bool pressed)
    {
        if (!pressed)
            return;

        if (session?.AttachedEntity is not { } uid)
            return;

        if (!TryComp<ManualBreathingComponent>(uid, out var comp))
            return;

        comp.NeedsInhale = true;
        comp.LastManualBreathTime = _timing.CurTime;
        Dirty(uid, comp);
    }

    public float GetBreathProgress(ManualBreathingComponent comp)
    {
        if (comp.BreathCooldown <= 0f)
            return 0f;

        var timeSinceBreath = (float) (_timing.CurTime - comp.LastManualBreathTime).TotalSeconds;
        return timeSinceBreath / comp.BreathCooldown;
    }


    public float GetBreathUrgency(ManualBreathingComponent comp)
    {
        if (comp.NeedsInhale)
            return 0f;

        var timeSinceBreath = (float) (_timing.CurTime - comp.LastManualBreathTime).TotalSeconds;

        if (timeSinceBreath < comp.BlurStartTime)
            return 0f;

        return (timeSinceBreath - comp.BlurStartTime) * comp.BlurGrowthRate;
    }
}
