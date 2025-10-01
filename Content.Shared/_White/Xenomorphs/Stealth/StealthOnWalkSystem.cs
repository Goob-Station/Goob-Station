using Content.Shared.Movement.Events;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;

namespace Content.Shared._White.Xenomorphs.Stealth;

public sealed class StealthOnWalkSystem : EntitySystem
{
    [Dependency] private readonly SharedStealthSystem _stealth = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StealthOnWalkComponent, SprintingInputEvent>(OnSprintingInput);
    }

    private void OnSprintingInput(EntityUid uid, StealthOnWalkComponent component, SprintingInputEvent args)
    {
        if (!TryComp<StealthComponent>(uid, out var stealth) || stealth.Enabled == !args.Entity.Comp.Sprinting)
            return;

        _stealth.SetEnabled(uid, !args.Entity.Comp.Sprinting, stealth);
        component.Stealth = stealth.Enabled;
    }
}
