using Content.Goobstation.Shared.Disease;
using Content.Goobstation.Shared.Virology;
using Content.Shared.Interaction;

namespace Content.Goobstation.Server.Virology;

public sealed class VaccineSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<VaccineComponent, AfterInteractEvent>(OnAfterInteract);
        base.Initialize();
    }

    private void OnAfterInteract(Entity<VaccineComponent> ent, ref AfterInteractEvent args)
    {
        if (!TryComp<ImmunityComponent>(args.Target, out var immunity)
            || ent.Comp.Genotype == null
            || ent.Comp.Used)
            return;

        immunity.ImmuneTo.Add(ent.Comp.Genotype.Value);
        ent.Comp.Used = true;

        _audio.PlayPredicted(component.InjectSound, target, user);
    }
}
