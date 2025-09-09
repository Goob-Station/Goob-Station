using Content.Shared.Chemistry.Hypospray.Events;
using Content.Shared._CorvaxGoob.MedipenRefiller;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;
using Content.Shared.Chemistry.EntitySystems.Hypospray;

namespace Content.Server._CorvaxGoob.MedipenRefiller;

public sealed class MedipenSystem : EntitySystem
{
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;

    private static readonly ProtoId<TagPrototype> TrashTag = "Trash";
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MedipenComponent, AfterHyposprayInjectsEvent>(OnMedipenUse);
        SubscribeLocalEvent<MedipenComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<MedipenComponent> entity, ref MapInitEvent args)
    {
        _appearance.SetData(entity, MedipenVisualLayer.Fill, true);
        UpdateAppearance(entity);
    }

    private void OnMedipenUse(Entity<MedipenComponent> entity, ref AfterHyposprayInjectsEvent args)
    {
        entity.Comp.Used = true;
        UpdateAppearance(entity);
    }

    private void UpdateAppearance(Entity<MedipenComponent> entity)
    {
        if (!entity.Comp.Used)
            return;

        if (!_solutionContainer.TryGetSolution(entity.Owner, MedipenRefillerSystem.MedipenSolutionName, out var solutionComo, out var solution))
            return;

        if (solution.Volume != 0)
        {
            _appearance.SetData(entity, MedipenVisualLayer.Fill, true);
            return;
        }

        if (entity.Comp.TrashOnUse)
            _tag.AddTag(entity.Owner, TrashTag);

        if (entity.Comp.SetTrashSpriteOnUse)
            _appearance.SetData(entity, MedipenVisualLayer.Fill, false);
    }
}
