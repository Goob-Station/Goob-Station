using Content.Shared.Interaction.Events;
using Content.Shared.Popups;

namespace Content.Shared._White.Blink;

public abstract class SharedBlinkSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlinkComponent, UseInHandEvent>(OnUseInHand);
    }

    private void OnUseInHand(Entity<BlinkComponent> ent, ref UseInHandEvent args)
    {
        if (args.Handled)
            return;

        ent.Comp.IsActive = !ent.Comp.IsActive;
        var message = ent.Comp.IsActive ? "blink-activated-message" : "blink-deactivated-message";
        _popup.PopupClient(Loc.GetString(message), args.User);
        Dirty(ent);
        args.Handled = true;
    }
}
