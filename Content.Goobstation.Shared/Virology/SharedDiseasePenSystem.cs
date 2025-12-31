using Content.Goobstation.Shared.Disease;
using Content.Goobstation.Shared.Disease.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Robust.Shared.Audio.Systems;

namespace Content.Goobstation.Shared.Virology;

public sealed class SharedDiseasePenSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedDiseaseSystem _disease = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<DiseasePenComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<DiseasePenComponent, DiseasePenInjectEvent>(OnInjectEvent);
        base.Initialize();
    }

    private void OnInjectEvent(Entity<DiseasePenComponent> ent, ref DiseasePenInjectEvent args)
    {
        if (args.Target == null
            || ent.Comp.DiseaseUid == null
            || !_disease.TryInfect(args.Target.Value, ent.Comp.DiseaseUid.Value))
            return;

        ent.Comp.Used = true;
        _audio.PlayPredicted(ent.Comp.InjectSound, args.User, args.User);
        _appearance.SetData(ent, DiseasePenVisuals.Used, true);
    }

    private void OnAfterInteract(Entity<DiseasePenComponent> ent, ref AfterInteractEvent args)
    {
        if (ent.Comp.Vaccine)
            TryVaccinate(ent, args);
        else
            TryInject(ent, args);
    }

    private void TryInject(Entity<DiseasePenComponent> ent, AfterInteractEvent args)
    {
        if (args.Target == null || ent.Comp.DiseaseUid == null || ent.Comp.Used)
            return;

        var doAfterArgs = new DoAfterArgs(EntityManager, args.User, ent.Comp.InjectTime, new DiseasePenInjectEvent(), ent, target: args.Target, used: ent)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            NeedHand = true,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void TryVaccinate(Entity<DiseasePenComponent> ent, AfterInteractEvent args)
    {
        if (!TryComp<Disease.Components.ImmunityComponent>(args.Target, out var immunity)
            || ent.Comp.Genotype == null
            || ent.Comp.Used
            || args.Target == null)
            return;

        immunity.ImmuneTo.Add(ent.Comp.Genotype.Value);
        ent.Comp.Used = true;

        _audio.PlayPredicted(ent.Comp.InjectSound, args.User, args.User);
        _appearance.SetData(ent, DiseasePenVisuals.Used, true);
    }
}
