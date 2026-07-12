using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs;
using Content.Shared._Impstation.Consume.Components;

namespace Content.Shared._Impstation.Consume;

public sealed class ConsumedSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ConsumedComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<ConsumedComponent, MobStateChangedEvent>(OnMobStateChange);
    }
    private void OnExamine(Entity<ConsumedComponent> ent, ref ExaminedEvent args)
    {

        var consumeIndex = 0;
        switch (ent.Comp.ConsumedValue)
        {
            case <= 0.25f:
                consumeIndex = 1;
                break;
            case <= 0.75f:
                consumeIndex = 2;
                break;
            case <= 1.0f:
                consumeIndex = 3;
                break;
            case <= 2.0f:
                consumeIndex = 4;
                break;
        }

        args.PushMarkup(Loc.GetString($"consumed-onexamine-{consumeIndex}",
            ("target", Identity.Entity(ent, EntityManager))));

    }
    private void OnMobStateChange(Entity<ConsumedComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Alive)
            RemComp<ConsumedComponent>(ent);
    }
}
