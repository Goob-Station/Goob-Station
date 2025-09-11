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

    private void OnBloodWritingAction(Entity<BloodWritingComponent> ent, ref BloodWritingEvent args)
    {
        var uid = ent.Owner;
        var comp = ent.Comp;

        if (args.Handled)
            return;

        if (!TryComp<HandsComponent>(uid, out var hands))
            return;

        if (comp.BloodCrayon != null)
        {
            // Disable blood writing
            if (_handsSystem.TryGetHand(uid, "crayon", out _))
                _handsSystem.RemoveHand(uid, "crayon", hands);
            PredictedQueueDel(comp.BloodCrayon);
            comp.BloodCrayon = null;
            Dirty(ent);
        }
        else
        {
            if (!_handsSystem.TryGetHand(uid, "crayon", out _))
                _handsSystem.AddHand(uid, "crayon", HandLocation.Middle);

            if (hands.ActiveHand == null
                || hands.ActiveHand.Container == null)
                return;

            var crayon = PredictedSpawnInContainerOrDrop(
                "CrayonBlood",
                hands.ActiveHand.Container.Owner,
                "crayon");

            comp.BloodCrayon = crayon;
            Dirty(ent);
            EnsureComp<UnremoveableComponent>(crayon);
        }

        args.Handled = true;
    }

    private void OnCrayonUse(EntityUid uid, BloodCrayonComponent comp, AfterInteractEvent args)
    {
        if (args.Handled)
            return;

        //TO DO: Add check to see if user has Wraith component

        //TO DO: Reduce WP after using the skill

    }
}
