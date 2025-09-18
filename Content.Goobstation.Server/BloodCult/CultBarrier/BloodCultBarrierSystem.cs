using Content.Server.Popups;
using Content.Shared.Interaction;

namespace Content.Goobstation.Server.BloodCult.CultBarrier;

public sealed class BloodCultBarrierSystem : EntitySystem
{
    [Dependency] private readonly PopupSystem _popup = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BloodCultBarrierComponent, InteractUsingEvent>(OnInteract);
    }

    private void OnInteract(Entity<BloodCultBarrierComponent> ent, ref InteractUsingEvent args)
    {
        if (!HasComp<Goobstation.Shared.BloodCult.Runes.RuneDrawerComponent>(args.Used) || !HasComp<Goobstation.Shared.BloodCult.BloodCultist.BloodCultistComponent>(args.User))
            return;

        _popup.PopupEntity("cult-barrier-destroyed", args.User, args.User);
        Del(args.Target);
    }
}
