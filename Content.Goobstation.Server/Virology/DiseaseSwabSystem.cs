using Content.Goobstation.Shared.Disease.Systems;
using Content.Goobstation.Shared.Virology;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Server.Popups;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Virology;

public sealed class DiseaseSwabSystem : EntitySystem
{
    [Dependency] private readonly SharedDiseaseSystem _disease = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DiseaseSwabComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<DiseaseSwabComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<DiseaseSwabComponent, ExaminedEvent>(OnExamined);
    }

    private void OnAfterInteract(Entity<DiseaseSwabComponent> ent, ref AfterInteractEvent args)
    {
        if (!args.CanReach || args.Target == null)
            return;

        // Target must have diseases
        if (!TryComp<Shared.Disease.Components.DiseaseCarrierComponent>(args.Target, out var carrier))
        {
            _popup.PopupEntity(Loc.GetString("disease-swab-cant-swab", ("target", args.Target)), args.User, args.User);
            return;
        }

        _popup.PopupEntity(Loc.GetString("disease-swab-swabbed",
            ("target", args.Target == args.User ? Loc.GetString("disease-swab-yourself") : args.Target )),
            args.User,
            args.User);

        if(args.Target != args.User)
            _popup.PopupEntity(Loc.GetString("disease-swab-swabbed-by", ("user", args.User)), args.Target.Value, args.Target.Value);

        if (carrier.Diseases.Count == 0)
            return;

        // Pick a random disease
        var diseaseToClone = _random.Pick(carrier.Diseases);
        SetDisease((ent, ent.Comp), diseaseToClone);
        args.Handled = true;
    }

    private void OnExamined(Entity<DiseaseSwabComponent> ent, ref ExaminedEvent args)
    {
        if (ent.Comp.DiseaseUid != null)
            args.PushMarkup(Loc.GetString("disease-swab-unclean"));
    }

    private void OnShutdown(Entity<DiseaseSwabComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.DiseaseUid != null)
            QueueDel(ent.Comp.DiseaseUid);
    }

    private void SetDisease(Entity<DiseaseSwabComponent?> ent, EntityUid? diseaseUid)
    {
        if (!Resolve(ent, ref ent.Comp))
            return;

        // if we're setting to the disease we already have
        if (diseaseUid == ent.Comp.DiseaseUid)
            return;

        Dirty(ent, ent.Comp);

        // delete the old disease
        QueueDel(ent.Comp.DiseaseUid);
        if (diseaseUid != null)
            ent.Comp.DiseaseUid = _disease.TryClone(diseaseUid.Value);
    }
}
