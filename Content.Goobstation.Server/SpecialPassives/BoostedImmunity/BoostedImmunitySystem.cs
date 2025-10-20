using Content.Goobstation.Common.Traits;
using Content.Goobstation.Common.Traits.Components;
using Content.Goobstation.Shared.SpecialPassives.BoostedImmunity.Components;
using Content.Goobstation.Shared.Traits.Components;
using Content.Server.Traits.Assorted;
using Content.Shared.Mobs.Components;
using Content.Shared.Overlays;
using Content.Shared.Speech.Muting;
using Content.Shared.Traits.Assorted;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.SpecialPassives.BoostedImmunity;

public sealed class BoostedImmunitySystem : SharedBoostedImmunitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    private EntityQuery<MobStateComponent> _mobStateQuery;

    private static readonly List<Type> Disabilities = new()
    {
        typeof(PermanentBlindnessComponent),
        typeof(NarcolepsyComponent), // narcolepsy is the sole reason this part is in server
        typeof(UnrevivableComponent),
        typeof(BlackAndWhiteOverlayComponent),
        typeof(MutedComponent),
        typeof(ParacusiaComponent),
        typeof(PainNumbnessComponent),
        typeof(LegsParalyzedComponent),
        typeof(MovementImpairedComponent),
        typeof(SocialAnxietyComponent),
        typeof(DeafComponent)
    };

    public override void Initialize()
    {
        base.Initialize();

        _mobStateQuery = GetEntityQuery<MobStateComponent>();

        SubscribeLocalEvent<BoostedImmunityComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<BoostedImmunityComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.UpdateTimer = _timing.CurTime + ent.Comp.UpdateDelay;

        if (ent.Comp.Duration.HasValue)
            ent.Comp.MaxDuration = _timing.CurTime + TimeSpan.FromSeconds((double) ent.Comp.Duration);

        if (_mobStateQuery.TryComp(ent, out var state))
            ent.Comp.Mobstate = state.CurrentState;

        if (ent.Comp.RemoveDisabilities)
            foreach (var disability in Disabilities)
                RemComp(ent, disability);

        Cycle(ent);
    }
}
