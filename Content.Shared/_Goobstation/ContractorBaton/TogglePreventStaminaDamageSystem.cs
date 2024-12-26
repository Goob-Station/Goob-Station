using Content.Shared.Damage.Events;
using Content.Shared.Item.ItemToggle;

namespace Content.Shared._Goobstation.ContractorBaton;

public sealed class TogglePreventStaminaDamageSystem : EntitySystem
{
    [Dependency] private readonly ItemToggleSystem _toggle = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TogglePreventStaminaDamageComponent, StaminaDamageOnHitAttemptEvent>(OnStaminaHitAttempt);
    }

    private void OnStaminaHitAttempt(Entity<TogglePreventStaminaDamageComponent> ent,
        ref StaminaDamageOnHitAttemptEvent args)
    {
        if (!_toggle.IsActivated(ent.Owner))
            args.Cancelled = true;
    }
}
