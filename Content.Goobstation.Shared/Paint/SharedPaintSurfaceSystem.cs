using System.Diagnostics.CodeAnalysis;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Crayon;
using Content.Shared.Examine;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Paint;

public abstract partial class SharedPaintSurfaceSystem : EntitySystem
{
    [Dependency] protected IGameTiming Timing = default!;
    [Dependency] private SharedHandsSystem _hands = default!;
    [Dependency] private IPrototypeManager _proto = default!;
    [Dependency] protected SharedSolutionContainerSystem Solutions = default!;
    [Dependency] private SharedUserInterfaceSystem _ui = default!;

    private readonly HashSet<int> _strokePixels = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PaintSurfaceComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<PaintSurfaceComponent, ComponentGetState>(OnGetState);
        SubscribeLocalEvent<PaintSurfaceComponent, ComponentHandleState>(OnHandleState);
        SubscribeLocalEvent<PaintSurfaceComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<PaintSurfaceComponent, ExaminedEvent>(OnExamined);
    }

    private void OnInit(Entity<PaintSurfaceComponent> ent, ref ComponentInit args)
    {
        if (ent.Comp.Image != null)
            return;

        ent.Comp.Image = new PaintImage(ent.Comp.Size.X, ent.Comp.Size.Y, ent.Comp.Background);
        ent.Comp.ChunkModified = new GameTick[PaintChunks.Count(ent.Comp.Size)];
    }

    private void OnGetState(Entity<PaintSurfaceComponent> ent, ref ComponentGetState args)
    {
        if (ent.Comp.Image is not { } image)
            return;

        if (args.FromTick <= ent.Comp.CreationTick)
        {
            args.State = new PaintSurfaceComponentState(image.Size,
                image.Palette.ToArray(),
                (byte[]) image.Pixels.Clone(),
                ent.Comp.Finished,
                ent.Comp.Title,
                ent.Comp.Artist);
            return;
        }

        var palette = ent.Comp.PaletteModified >= args.FromTick ? image.Palette.ToArray() : null;
        var chunks = new Dictionary<int, byte[]>();
        for (var i = 0; i < ent.Comp.ChunkModified.Length; i++)
        {
            if (ent.Comp.ChunkModified[i] >= args.FromTick)
                chunks[i] = PaintChunks.Read(image.Pixels, image.Size, i);
        }

        args.State = new PaintSurfaceComponentDeltaState(palette, chunks, ent.Comp.Finished, ent.Comp.Title, ent.Comp.Artist);
    }

    private void OnHandleState(Entity<PaintSurfaceComponent> ent, ref ComponentHandleState args)
    {
        switch (args.Current)
        {
            case PaintSurfaceComponentDeltaState delta:
            {
                if (ent.Comp.Image is not { } image)
                    return;

                ent.Comp.Finished = delta.Finished;
                ent.Comp.Title = delta.Title;
                ent.Comp.Artist = delta.Artist;

                if (delta.Palette != null)
                {
                    image.Palette.Clear();
                    image.Palette.AddRange(delta.Palette);
                }

                var chunks = new List<int>(delta.Chunks.Count);
                var count = PaintChunks.Count(image.Size);
                foreach (var (chunk, data) in delta.Chunks)
                {
                    if (chunk < 0 || chunk >= count)
                        continue;

                    PaintChunks.Write(image.Pixels, image.Size, chunk, data);
                    chunks.Add(chunk);
                }

                var ev = new PaintSurfaceUpdatedEvent(chunks);
                RaiseLocalEvent(ent, ref ev);
                break;
            }
            case PaintSurfaceComponentState state:
            {
                ent.Comp.Size = state.Size;
                ent.Comp.Finished = state.Finished;
                ent.Comp.Title = state.Title;
                ent.Comp.Artist = state.Artist;
                ent.Comp.Image = new PaintImage(state.Size.X,
                    state.Size.Y,
                    new List<Color>(state.Palette),
                    (byte[]) state.Pixels.Clone());
                ent.Comp.ChunkModified = new GameTick[PaintChunks.Count(state.Size)];

                var ev = new PaintSurfaceUpdatedEvent(null);
                RaiseLocalEvent(ent, ref ev);
                break;
            }
        }
    }

    private void OnInteractUsing(EntityUid uid, PaintSurfaceComponent component, InteractUsingEvent args)
    {
        if (args.Handled || !HasComp<PaintToolComponent>(args.Used) || !_ui.HasUi(uid, PaintUiKey.Key))
            return;

        _ui.OpenUi(uid, PaintUiKey.Key, args.User);
        args.Handled = true;
    }

    private void OnExamined(Entity<PaintSurfaceComponent> ent, ref ExaminedEvent args)
    {
        if (!ent.Comp.Finished)
            return;

        args.PushMarkup(Loc.GetString("paint-examine-finished",
            ("title", FormattedMessage.EscapeText(ent.Comp.Title ?? Loc.GetString("paint-untitled"))),
            ("artist", FormattedMessage.EscapeText(ent.Comp.Artist ?? Loc.GetString("paint-unknown-artist")))));
    }

    public bool TryGetActiveTool(EntityUid user, [NotNullWhen(true)] out Entity<PaintToolComponent>? tool)
    {
        tool = null;
        if (_hands.GetActiveItem(user) is not { } item || !TryComp<PaintToolComponent>(item, out var comp))
            return false;

        tool = (item, comp);
        return true;
    }

    public bool AllowsColorPicker(Entity<PaintToolComponent> tool)
    {
        return tool.Comp.ColorMode switch
        {
            PaintColorMode.Picker => true,
            PaintColorMode.Crayon => TryComp<CrayonComponent>(tool, out var crayon) && crayon.SelectableColor,
            _ => false,
        };
    }

    public Color GetToolColor(Entity<PaintToolComponent> tool, Color requested)
    {
        if (AllowsColorPicker(tool))
            return PaintImage.Quantize(requested);

        if (tool.Comp.ColorMode == PaintColorMode.Crayon && TryComp<CrayonComponent>(tool, out var crayon))
            return PaintImage.Quantize(crayon.Color);

        if (tool.Comp.ColorMode == PaintColorMode.Solution
            && Solutions.TryGetSolution(tool.Owner, tool.Comp.Solution, out _, out var solution)
            && solution.Volume > 0)
            return PaintImage.Quantize(solution.GetColor(_proto));

        return PaintImage.Quantize(tool.Comp.Color);
    }

    public bool HasPaint(Entity<PaintToolComponent> tool)
    {
        if (tool.Comp.ColorMode != PaintColorMode.Solution)
            return true;

        return Solutions.TryGetSolution(tool.Owner, tool.Comp.Solution, out _, out var solution) && solution.Volume > 0;
    }

    public virtual bool SetImage(Entity<PaintSurfaceComponent> ent, PaintImage image)
    {
        if (image.Size != ent.Comp.Size
            || image.Pixels.Length != image.Width * image.Height
            || image.Palette.Count == 0
            || image.Palette.Count > PaintImage.MaxPaletteSize)
            return false;

        ent.Comp.Image = image;
        ent.Comp.PaletteModified = Timing.CurTick;
        ent.Comp.ChunkModified = new GameTick[PaintChunks.Count(image.Size)];
        Array.Fill(ent.Comp.ChunkModified, Timing.CurTick);
        Dirty(ent);
        return true;
    }

    public int ApplyStroke(
        Entity<PaintSurfaceComponent> ent,
        ReadOnlySpan<Vector2i> points,
        PaintShape shape,
        int brushSize,
        PaintBrushStyle style,
        int seed,
        Color color,
        bool erase,
        Dictionary<int, (byte Old, byte New)>? changes = null)
    {
        if (ent.Comp.Image is not { } image)
            return 0;

        _strokePixels.Clear();
        PaintBrush.Rasterize(points, shape, brushSize, style, seed, image.Size, _strokePixels);
        if (_strokePixels.Count == 0)
            return 0;

        var paletteChanged = false;
        byte entry = 0;
        if (!erase)
        {
            entry = image.FindOrAddColor(color, out paletteChanged);
            if (paletteChanged)
                ent.Comp.PaletteModified = Timing.CurTick;
        }

        var changed = 0;
        foreach (var pixel in _strokePixels)
        {
            var old = image.Pixels[pixel];
            if (old == entry)
                continue;

            if (changes != null)
                changes[pixel] = changes.TryGetValue(pixel, out var existing) ? (existing.Old, entry) : (old, entry);

            image.Pixels[pixel] = entry;
            ent.Comp.ChunkModified[PaintChunks.ChunkOf(image.Size, pixel)] = Timing.CurTick;
            changed++;
        }

        if (changed > 0 || paletteChanged)
            Dirty(ent);

        return changed;
    }

    public int RestorePixels(Entity<PaintSurfaceComponent> ent, Dictionary<int, (byte Old, byte New)> changes)
    {
        if (ent.Comp.Image is not { } image)
            return 0;

        var restored = 0;
        foreach (var (pixel, (old, current)) in changes)
        {
            if (pixel < 0 || pixel >= image.Pixels.Length || image.Pixels[pixel] != current || old >= image.Palette.Count)
                continue;

            image.Pixels[pixel] = old;
            ent.Comp.ChunkModified[PaintChunks.ChunkOf(image.Size, pixel)] = Timing.CurTick;
            restored++;
        }

        if (restored > 0)
            Dirty(ent);

        return restored;
    }
}
