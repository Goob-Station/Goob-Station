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
        SubscribeLocalEvent<BloodWritingComponent, BloodWritingEvent>(OnBloodWritingAction);
    }

    private void OnBloodWritingAction(EntityUid uid, BloodWritingComponent component, BloodWritingEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<HandsComponent>(uid, out var hands))
            return;

        if (component.BloodWriting != null)
        {
            // Disable blood writing
            _handsSystem.RemoveHands(uid);
            QueueDel(component.BloodWriting);
            component.BloodWriting = null;
        }
        else
        {
            _handsSystem.AddHand(uid, "crayon", HandLocation.Middle);
            var crayon = Spawn("CrayonBlood");
            component.BloodWriting = crayon;
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
