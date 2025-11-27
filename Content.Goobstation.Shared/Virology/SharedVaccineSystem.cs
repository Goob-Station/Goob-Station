using Content.Goobstation.Shared.Disease;
using Content.Goobstation.Shared.Virology;
using Content.Shared.Interaction;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Shared.Virology;

public sealed class SharedVaccineSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<VaccineComponent, AfterInteractEvent>(OnAfterInteract);
        base.Initialize();
    }

    private void OnAfterInteract(Entity<VaccineComponent> ent, ref AfterInteractEvent args)
    {
        /*
        if (!TryComp<ImmunityComponent>(args.Target, out var immunity)
            || ent.Comp.Genotype == null
            || ent.Comp.Used
            || args.Target == null)
            return;

        immunity.ImmuneTo.Add(ent.Comp.Genotype.Value);
        ent.Comp.Used = true;
        */

        _audio.PlayPredicted(ent.Comp.InjectSound, args.User, args.User);
        _appearance.SetData(ent, VaccineVisuals.Used, true);
    }
}
