using Content.Shared._Shitmed.Body.Organ;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.PowerCell;

namespace Content.Goobstation.Shared.Augments;

public sealed class AugmentPowerDrawSystem : EntitySystem
{
    [Dependency] private readonly AugmentSystem _augment = default!;
    [Dependency] private readonly ItemToggleSystem _toggle = default!;
    [Dependency] private readonly SharedAugmentPowerCellSystem _augmentPower = default!;
    [Dependency] private readonly SharedPowerCellSystem _powerCell = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AugmentPowerDrawComponent, GetAugmentsPowerDrawEvent>(OnGetPowerDraw);
        SubscribeLocalEvent<AugmentPowerDrawComponent, OrganEnableChangedEvent>(OnEnableChanged);
        SubscribeLocalEvent<AugmentPowerDrawComponent, ItemToggleActivateAttemptEvent>(OnActivateAttempt);
        SubscribeLocalEvent<AugmentPowerDrawComponent, ItemToggledEvent>(OnToggled);
    }

    private void OnGetPowerDraw(Entity<AugmentPowerDrawComponent> ent, ref GetAugmentsPowerDrawEvent args)
    {
        if (_toggle.IsActivated(ent.Owner))
            args.TotalDraw += ent.Comp.Draw;
    }

    private void OnEnableChanged(Entity<AugmentPowerDrawComponent> ent, ref OrganEnableChangedEvent args)
    {
        _toggle.TrySetActive(ent.Owner, args.Enabled);
    }

    private void OnActivateAttempt(Entity<AugmentPowerDrawComponent> ent, ref ItemToggleActivateAttemptEvent args)
    {
        if (_augment.GetBody(ent) is not {} body ||
            _augmentPower.GetBodyAugment(body) is not {} slot ||
            !_powerCell.HasDrawCharge(slot))
        {
            args.Cancelled = true;
        }
    }

    private void OnToggled(Entity<AugmentPowerDrawComponent> ent, ref ItemToggledEvent args)
    {
        if (_augment.GetBody(ent) is {} body && _augmentPower.GetBodyAugment(body) is {} slot)
            _augmentPower.UpdateDrawRate(slot.Owner);
    }
}
