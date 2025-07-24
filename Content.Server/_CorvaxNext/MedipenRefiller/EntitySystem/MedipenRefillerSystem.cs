using Content.Shared._CorvaxNext.MedipenRefiller;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Containers.ItemSlots;
using Microsoft.Extensions.DependencyModel;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Containers;

namespace Content.Server._CorvaxNext.MedipenRefiller;

public sealed class MedipenRefillerSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly EntityManager _entityManage = default!;

    public const string InputSlotName = "beakerSlot";
    public const string MedipenSlotName = "medipenSlot";

    public const string MedipenSolutionName = "pen";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MedipenRefillerComponent, ComponentStartup>(SubscribeUpdateUiState);
        SubscribeLocalEvent<MedipenRefillerComponent, SolutionContainerChangedEvent>(SubscribeUpdateUiState);
        SubscribeLocalEvent<MedipenRefillerComponent, EntRemovedFromContainerMessage>(SubscribeUpdateUiState);

        SubscribeLocalEvent<MedipenRefillerComponent, EntInsertedIntoContainerMessage>(OnEntityEnserted);
        SubscribeLocalEvent<MedipenRefillerComponent, BoundUIOpenedEvent>(OnUiOpen);

        SubscribeLocalEvent<MedipenRefillerComponent, MedipenRefillerApplySettingsMessage>(OnMedipenNewSettings);
        SubscribeLocalEvent<MedipenRefillerComponent, MedipenRefillerFillMedipenMessage>(OnMedipenFill);
    }

    public void OnMedipenNewSettings(Entity<MedipenRefillerComponent> entity, ref MedipenRefillerApplySettingsMessage args)
    {
        entity.Comp.CurrentLabel = args.Label;
        entity.Comp.CurrentVolume = args.Volume;

        ClickSound(entity);

        var medipenSlot = _itemSlots.GetItemOrNull(entity.Owner, MedipenSlotName);

        if (!TryComp<MedipenComponent>(medipenSlot, out var medipenComponent))
            return;

        if (medipenSlot is null)
            return;

        if (entity.Comp.CurrentColor != args.Color || medipenComponent.Color != args.Color)
            _audio.PlayPvs(entity.Comp.RecolorMedipenSound, entity);

        entity.Comp.CurrentColor = args.Color;
        Dirty(entity);

        _appearance.SetData(medipenSlot.Value, MedipenColorLayer.Empty, args.Color);
        _appearance.SetData(medipenSlot.Value, MedipenColorLayer.Fill, args.Color);

        if (args.Label != "")
            _meta.SetEntityName(medipenSlot.Value, $"{Loc.GetString(medipenComponent.DefaultLabel)} ({args.Label})");
        else
            _meta.SetEntityName(medipenSlot.Value, Loc.GetString(medipenComponent.DefaultLabel));
    }

    public void OnMedipenFill(Entity<MedipenRefillerComponent> entity, ref MedipenRefillerFillMedipenMessage args)
    {
        ClickSound(entity);

        var medipenSlot = _itemSlots.GetItemOrNull(entity.Owner, MedipenSlotName);

        if (medipenSlot is null)
            return;

        var inputContainer = _itemSlots.GetItemOrNull(entity.Owner, InputSlotName);

        if (inputContainer is null)
            return;

        if (!_solutionContainer.TryGetSolution(medipenSlot.Value, MedipenSolutionName, out var medipenSolutionComp, out var medipenSolution))
            return;

        if (medipenSolution.MaxVolume.Int() <= 0)
            return;

        if (medipenSolution.Volume == medipenSolution.MaxVolume)
            return;

        if (!_solutionContainer.TryGetFitsInDispenser(inputContainer.Value, out var inputContainerComp, out var inputSolution))
            return;

        entity.Comp.CurrentVolume = Math.Clamp(args.Volume, 1, medipenSolution.MaxVolume.Int());

        var injectVolume = Math.Clamp(entity.Comp.CurrentVolume, 1, medipenSolution.MaxVolume.Int() - medipenSolution.Volume.Int());

        _solutionContainer.TryAddSolution(medipenSolutionComp.Value, inputSolution.SplitSolution(injectVolume));

        _audio.PlayPvs(entity.Comp.FillMedipenSound, entity);
        UpdateUiState(entity, true);
    }

    public void OnUiOpen(Entity<MedipenRefillerComponent> entity, ref BoundUIOpenedEvent args)
    {
        UpdateUiState(entity);
    }

    public void OnEntityEnserted(Entity<MedipenRefillerComponent> entity, ref EntInsertedIntoContainerMessage args)
    {
        if (HasComp<MedipenComponent>(args.Entity) && _entityManage.TryGetComponent<MetaDataComponent>(args.Entity, out var meta) && meta.EntityPrototype is not null)
            entity.Comp.PreviewPrototype = meta.EntityPrototype.ID;

        UpdateUiState(entity);
    }

    public void SubscribeUpdateUiState<T>(Entity<MedipenRefillerComponent> entity, ref T args)
    {
        UpdateUiState(entity, true);
    }

    private void UpdateUiState(Entity<MedipenRefillerComponent> entity, bool ignoreColor = false)
    {
        var inputSlot = _itemSlots.GetItemOrNull(entity.Owner, InputSlotName);
        var medipenSlot = _itemSlots.GetItemOrNull(entity.Owner, MedipenSlotName);

        var state = new MedipenRefillerBoundUserInterfaceState(BuildInputContainerInfo(inputSlot), BuildMedipenContainerInfo(medipenSlot), entity.Comp.PreviewPrototype);

        _userInterface.SetUiState(entity.Owner, MedipenRefillerUiKey.Key, state);
    }

    private ContainerInfo? BuildInputContainerInfo(EntityUid? container)
    {
        if (container is not { Valid: true })
            return null;

        if (!TryComp(container, out FitsInDispenserComponent? fits) || !_solutionContainer.TryGetSolution(container.Value, fits.Solution, out _, out var solution))
            return null;

        return new ContainerInfo(Name(container.Value), solution.Volume, solution.MaxVolume)
        {
            Reagents = solution.Contents
        };
    }

    private ContainerInfo? BuildMedipenContainerInfo(EntityUid? container)
    {
        if (container is not { Valid: true })
            return null;

        if (!_solutionContainer.TryGetSolution(container.Value, MedipenSolutionName, out _, out var solution))
            return null;

        return new ContainerInfo(Name(container.Value), solution.Volume, solution.MaxVolume)
        {
            Reagents = solution.Contents
        };
    }
    private void ClickSound(Entity<MedipenRefillerComponent> entity)
    {
        _audio.PlayPvs(entity.Comp.ClickSound, entity, AudioParams.Default.WithVolume(-2f));
    }
}
