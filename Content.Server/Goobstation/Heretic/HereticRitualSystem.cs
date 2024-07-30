using Content.Server.Heretic.Components;
using Content.Shared.Heretic;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;

namespace Content.Server.Heretic;

public sealed partial class HereticRitualSystem : SharedHereticRitualSystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HereticRitualRuneComponent, InteractHandEvent>(OnInteract);
        SubscribeLocalEvent<HereticRitualRuneComponent, InteractUsingEvent>(OnInteractUsing);
    }

    private void OnInteract(Entity<HereticRitualRuneComponent> ent, ref InteractHandEvent args)
    {
        if (!TryComp<HereticComponent>(args.User, out var heretic))
            return;

        if (heretic.KnownRituals.Count == 0)
        {

            return;
        }
        if (heretic.ChosenRitual != null)
        {
            var index = heretic.KnownRituals.FindIndex(m => m == heretic.ChosenRitual) + 1;
            if (index >= heretic.KnownRituals.Count)
                index = 0;
            heretic.ChosenRitual = heretic.KnownRituals[index];

        }
    }
    private void OnInteractUsing(Entity<HereticRitualRuneComponent> ent, ref InteractUsingEvent args)
    {
        if (!TryComp<HereticComponent>(args.User, out var heretic))
            return;

        if (!TryComp<MansusGraspComponent>(args.Used, out var grasp))
            return;

        if (heretic.ChosenRitual == null)
        {

            return;
        }

        if (!TryDoRitual(args.User, ent, (ProtoId<HereticRitualPrototype>) heretic.ChosenRitual))
        {

            return;
        }
    }
}
