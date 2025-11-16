using Content.Shared._Shitmed.Body.Organ;
using Content.Shared.Body.Organ;

namespace Content.Goobstation.Shared.Augments;

public sealed class AugmentActivatableUISystem : EntitySystem
{
    [Dependency] private readonly AugmentSystem _augment = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AugmentActivatableUIComponent, AugmentActionEvent>(OnAugmentAction);
    }

    private void OnAugmentAction(Entity<AugmentActivatableUIComponent> augment, ref AugmentActionEvent args)
    {
        if (_augment.GetBody(augment) is not {} body ||
            augment.Comp.Key is not {} key ||
            !_ui.HasUi(augment, key))
            return;

        _ui.OpenUi(augment.Owner, augment.Comp.Key, body);
        args.Handled = true;
    }
}
