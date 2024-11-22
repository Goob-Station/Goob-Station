using Content.Server.Popups;
using Content.Shared.Interaction;
using Content.Shared.WhiteDream.BloodCult.BloodCultist;
using Content.Shared.WhiteDream.BloodCult.Runes;

namespace Content.Server.WhiteDream.BloodCult.CultBarrier;

public sealed class BloodCultBarrierSystem : EntitySystem
{
    [Dependency] private readonly PopupSystem _popup = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BloodCultBarrierComponent, InteractUsingEvent>(OnInteract);
    }

    private void OnInteract(Entity<BloodCultBarrierComponent> ent, ref InteractUsingEvent args)
    {
        if (!HasComp<RuneDrawerComponent>(args.Used) || !HasComp<BloodCultistComponent>(args.User))
            return;

        _popup.PopupEntity("cult-barrier-destroyed", args.User, args.User);
        Del(args.Target);
    }
}
