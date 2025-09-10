using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared.Crayon;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Components;
using Content.Shared.Popups;

namespace Content.Goobstation.Shared.Wraith.Systems;
public sealed class BloodCrayonSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodCrayonComponent, AfterInteractEvent>(OnCrayonUse, before: [typeof(SharedCrayonSystem)]);
        SubscribeLocalEvent<WraithComponent, WraithBloodWritingEvent>(OnBloodWritingAction);
    }

    private void OnBloodWritingAction(EntityUid uid, WraithComponent component, WraithBloodWritingEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<HandsComponent>(uid, out var hands))
            return;

        if (component.BloodCrayon != null)
        {
            // Disable blood writing
            _handsSystem.RemoveHands(uid);
            QueueDel(component.BloodCrayon);
            component.BloodCrayon = null;
        }
        else
        {
            _handsSystem.AddHand(uid, "crayon", HandLocation.Middle);
            var crayon = Spawn("CrayonBlood");
            component.BloodCrayon = crayon;
            _handsSystem.DoPickup(uid, hands.Hands["crayon"], crayon);
            EnsureComp<UnremoveableComponent>(crayon);
        }
    }

    private void OnCrayonUse(EntityUid uid, BloodCrayonComponent comp, AfterInteractEvent args)
    {
        if (args.Handled)
            return;

        //TO DO: Add check to see if user has Wraith component

        //TO DO: Reduce WP after using the skill

    }
}
