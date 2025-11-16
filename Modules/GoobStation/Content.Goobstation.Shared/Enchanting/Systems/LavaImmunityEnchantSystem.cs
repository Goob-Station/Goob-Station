using Content.Goobstation.Shared.Enchanting.Components;
using Content.Shared.StepTrigger.Components;
using Content.Shared.StepTrigger.Systems;

namespace Content.Goobstation.Shared.Enchanting.Systems;

/// <summary>
/// Cancels step triggering for <see cref="LavaImmunityEnchantComponent"/>.
/// </summary>
public sealed class LavaImmunityEnchantSystem : EntitySystem
{
    [Dependency] private readonly EnchantingSystem _enchanting = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LavaImmunityEnchantComponent, StepTriggerAttemptEvent>(OnStepTriggerAttempt);
    }

    private void OnStepTriggerAttempt(Entity<LavaImmunityEnchantComponent> ent, ref StepTriggerAttemptEvent args)
    {
        if (!TryComp<StepTriggerComponent>(args.Source, out var step))
            return;

        args.Cancelled |= ent.Comp.Group.IsValid(step);
    }
}
