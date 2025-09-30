using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Paper;
using Content.Shared.Storage.Components;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Books;

public abstract partial class SharedCustomBooksSystem : EntitySystem
{
    [Dependency] protected readonly SharedUserInterfaceSystem UI = default!;
    [Dependency] protected readonly SharedContainerSystem Container = default!;
    [Dependency] protected readonly SharedAppearanceSystem Appearance = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CustomBookComponent, MapInitEvent>(OnBookInit);
        SubscribeLocalEvent<CustomBookComponent, ExaminedEvent>(OnBookExamined);

        SubscribeLocalEvent<BookBinderComponent, ComponentInit>(OnBinderInit);
        SubscribeLocalEvent<BookBinderComponent, EntInsertedIntoContainerMessage>(OnEntInserted);
        SubscribeLocalEvent<BookBinderComponent, EntRemovedFromContainerMessage>(OnEntRemoved);
        SubscribeLocalEvent<BookBinderComponent, InteractUsingEvent>(OnBinderInteractUsing);

    }

    private void OnBookInit(Entity<CustomBookComponent> ent, ref MapInitEvent args)
    {
        if (!ent.Comp.Template)
            RegenerateBook(ent);
    }

    private void OnBookExamined(Entity<CustomBookComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("custom-book-examine-author", ("author", ent.Comp.Author)), -5);
        args.PushMarkup(Loc.GetString("custom-book-examine-genre", ("genre", ent.Comp.Genre)), -4);
    }

    private void OnBinderInit(Entity<BookBinderComponent> ent, ref ComponentInit args)
    {
        ent.Comp.PaperContainer = Container.EnsureContainer<Container>(ent.Owner, "binder_paper");
    }

    private void OnEntInserted(Entity<BookBinderComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (_net.IsServer)
            Appearance.SetData(ent.Owner, BookBinderVisuals.Inserting, true);
        ent.Comp.InsertingEnd = _timing.CurTime + TimeSpan.FromSeconds(1);
        UpdateBinderUi(ent);
    }

    private void OnEntRemoved(Entity<BookBinderComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        UpdateBinderUi(ent);
    }

    private void OnBinderInteractUsing(Entity<BookBinderComponent> ent, ref InteractUsingEvent args)
    {
        if (!TryComp<PaperComponent>(args.Used, out var paper) || _timing.ApplyingState)
            return;

        Container.Insert(args.Used, ent.Comp.PaperContainer);
    }

    protected void RegenerateBook(Entity<CustomBookComponent> ent)
    {
        if (ent.Comp.Binding != null)
            Appearance.SetData(ent, CustomBookVisuals.Visuals, ent.Comp.Binding);

        UI.SetUiState(ent.Owner, CustomBookUiKey.Key, new CustomBookUiState(ent.Comp.Pages));
    }

    protected void UpdateBinderUi(Entity<BookBinderComponent> ent)
    {
        Dictionary<NetEntity, string> pages = new();

        foreach (var item in ent.Comp.PaperContainer.ContainedEntities)
        {
            if (!TryComp<PaperComponent>(item, out var paper))
                return;

            pages.Add(GetNetEntity(item), paper.Content);
        }

        UI.SetUiState(ent.Owner, BookBinderUiKey.Key, new BookBinderUiState(pages));
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<BookBinderComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.InsertingEnd != TimeSpan.Zero && comp.InsertingEnd < _timing.CurTime)
            {
                comp.InsertingEnd = TimeSpan.Zero;
                Appearance.SetData(uid, BookBinderVisuals.Inserting, false);
            }
        }
    }
}
