using Content.Goobstation.Shared.Wraith.Components;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;

namespace Content.Goobstation.Shared.Wraith.Systems;
public sealed class BloodCrayonSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BloodWritingComponent, BloodWritingEvent>(OnBloodWritingAction);
    }

    private void OnBloodWritingAction(Entity<BloodWritingComponent> ent, ref BloodWritingEvent args)
    {
        if (args.Handled)
            return;

        if (ent.Comp.BloodCrayon == null)
        {
            _handsSystem.AddHand(ent.Owner, ent.Comp.HandName, HandLocation.Middle);

            var crayon = PredictedSpawnAtPosition(ent.Comp.BloodCrayonEntId, Transform(ent.Owner).Coordinates);
            _handsSystem.TryForcePickup(ent.Owner, crayon, ent.Comp.HandName, false);

            ent.Comp.BloodCrayon = crayon;
        }
        else
        {
            _handsSystem.RemoveHand(ent.Owner, ent.Comp.HandName);
            PredictedQueueDel(ent.Comp.BloodCrayon);
            ent.Comp.BloodCrayon = null;
        }
        Dirty(ent);

        args.Handled = true;
    }
}
