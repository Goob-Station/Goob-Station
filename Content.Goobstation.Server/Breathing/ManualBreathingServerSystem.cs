using Content.Goobstation.Shared.Breathing;
using Content.Server.Body.Systems;
using Content.Shared.Atmos;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Breathing;

public sealed class ManualBreathingServerSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ManualBreathingComponent, InhaleLocationEvent>(OnInhale);
    }

    private void OnInhale(Entity<ManualBreathingComponent> ent, ref InhaleLocationEvent args)
    {
        if (ent.Comp.NeedsInhale)
        {
            ent.Comp.NeedsInhale = false;
            Dirty(ent);
            return;
        }

        var timeSinceBreath = (float) (_timing.CurTime - ent.Comp.LastManualBreathTime).TotalSeconds;
        if (timeSinceBreath < ent.Comp.BreathCooldown)
            return;

        args.Gas = new GasMixture();
    }
}
