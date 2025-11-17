using Content.Goobstation.Shared.Slasher.Components;
using Content.Shared.Body.Components;
using Content.Shared.Standing;

namespace Content.Goobstation.Server.Slasher.Systems;

/// <summary>
/// Server-side rules for Slasher. On spawn, make them behave like a simple mob by removing BodyComponent
/// so damage does not route through limbs. Also ensure all relevant slasher ability components are present.
/// </summary>
public sealed class SlasherSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SlasherComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SlasherComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(Entity<SlasherComponent> ent, ref ComponentStartup args)
    {
        MakeSimple(ent.Owner);
        ApplyComponents(ent.Owner);
    }

    private void OnMapInit(Entity<SlasherComponent> ent, ref MapInitEvent args)
    {
        MakeSimple(ent.Owner);
        ApplyComponents(ent.Owner);
    }

    private void MakeSimple(EntityUid uid)
    {
        // Limb damage breaks soulsteal
        if (HasComp<BodyComponent>(uid))
            RemComp<BodyComponent>(uid);
    }

    private void ApplyComponents(EntityUid uid)
    {
        // Ensure all relevant slasher components.
        EnsureComp<SlasherSummonMacheteComponent>(uid);
        EnsureComp<SlasherIncorporealComponent>(uid);
        EnsureComp<SlasherBloodTrailComponent>(uid);
        EnsureComp<SlasherPossessionComponent>(uid);
        EnsureComp<SlasherRegenerateComponent>(uid);
        EnsureComp<SlasherStaggerAreaComponent>(uid);
        EnsureComp<SlasherSoulStealComponent>(uid);
        EnsureComp<SlasherSummonMeatSpikeComponent>(uid);
        // Allow standing without legs/body
        EnsureComp<IgnoreLegsForStandingComponent>(uid);
        EnsureComp<Unbuckleable>(uid);
    }
}
