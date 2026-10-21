using Content.Goobstation.Shared.Paint;
using Content.Shared.Charges.Components;
using Content.Shared.Charges.Systems;
using Content.Shared.Crayon;
using Content.Shared.GameTicking;
using Content.Shared.IdentityManagement;
using Content.Shared.Popups;

namespace Content.Goobstation.Server.Paint;

public sealed partial class PaintSurfaceSystem : SharedPaintSurfaceSystem
{
    private const int MaxMessagesPerSecond = 40;
    private const int MaxUndoSteps = 20;

    [Dependency] private PaintLogSystem _paintLog = default!;
    [Dependency] private SharedChargesSystem _charges = default!;
    [Dependency] private MetaDataSystem _metaData = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedUserInterfaceSystem _ui = default!;

    private readonly Dictionary<(EntityUid Surface, EntityUid User), int> _unlogged = new();
    private readonly Dictionary<(EntityUid Surface, EntityUid User), List<UndoStep>> _undo = new();
    private readonly Dictionary<EntityUid, (TimeSpan Start, int Count)> _rate = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PaintSurfaceComponent, BoundUIClosedEvent>(OnUiClosed);
        SubscribeLocalEvent<PaintSurfaceComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestartCleanup);

        Subs.BuiEvents<PaintSurfaceComponent>(PaintUiKey.Key,
            subs =>
            {
                subs.Event<PaintStrokeMessage>(OnStroke);
                subs.Event<PaintUndoMessage>(OnUndo);
                subs.Event<PaintFinishMessage>(OnFinish);
            });
    }

    private void OnRoundRestartCleanup(RoundRestartCleanupEvent ev)
    {
        _rate.Clear();
    }

    private bool CheckRate(EntityUid user)
    {
        var now = Timing.RealTime;
        if (!_rate.TryGetValue(user, out var entry) || now - entry.Start >= TimeSpan.FromSeconds(1))
        {
            _rate[user] = (now, 1);
            return true;
        }

        if (entry.Count >= MaxMessagesPerSecond)
            return false;

        _rate[user] = (entry.Start, entry.Count + 1);
        return true;
    }

    private void OnStroke(Entity<PaintSurfaceComponent> ent, ref PaintStrokeMessage args)
    {
        var user = args.Actor;
        if (CheckRate(user))
            TryStroke(ent, user, args);

        _ui.ServerSendUiMessage(ent.Owner, PaintUiKey.Key, new PaintStrokeAckMessage(args.Sequence, Timing.CurTick), user);
    }

    private void TryStroke(Entity<PaintSurfaceComponent> ent, EntityUid user, PaintStrokeMessage args)
    {
        if (ent.Comp.Finished
            || ent.Comp.Image is not { } image
            || !TryGetActiveTool(user, out var maybeTool)
            || !Enum.IsDefined(args.Shape)
            || !Enum.IsDefined(args.Style)
            || !PaintBrush.IsValidStroke(args.Points, args.Shape, image.Size))
            return;

        var tool = maybeTool.Value;
        if (args.BrushSize < tool.Comp.MinBrushSize
            || args.BrushSize > tool.Comp.MaxBrushSize
            || args.Erase && !tool.Comp.CanErase)
            return;

        if (HasComp<LimitedChargesComponent>(tool) && _charges.IsEmpty(tool.Owner) || !HasPaint(tool))
        {
            _popup.PopupEntity(Loc.GetString("paint-tool-empty", ("tool", tool.Owner)), user, user);
            return;
        }

        var key = (ent.Owner, user);
        var step = GetUndoStep(key, args.StrokeId);
        var color = GetToolColor(tool, args.Color);
        var changed = ApplyStroke(ent,
            args.Points,
            args.Shape,
            args.BrushSize,
            args.Style,
            args.Sequence,
            color,
            args.Erase,
            step.Changes);

        if (changed == 0)
            return;

        _unlogged[key] = _unlogged.GetValueOrDefault(key) + changed;
        UseCharges(tool, user, changed);
        UsePaint(tool, changed);
    }

    private void UsePaint(Entity<PaintToolComponent> tool, int pixels)
    {
        if (tool.Comp.ColorMode != PaintColorMode.Solution
            || tool.Comp.PixelsPerUnit <= 0
            || !Solutions.TryGetSolution(tool.Owner, tool.Comp.Solution, out var solution, out _))
            return;

        tool.Comp.PixelDebt += pixels;
        var units = tool.Comp.PixelDebt / tool.Comp.PixelsPerUnit;
        tool.Comp.PixelDebt %= tool.Comp.PixelsPerUnit;
        if (units > 0)
            Solutions.SplitSolution(solution.Value, units);
    }

    private UndoStep GetUndoStep((EntityUid Surface, EntityUid User) key, int strokeId)
    {
        if (!_undo.TryGetValue(key, out var steps))
        {
            steps = new List<UndoStep>();
            _undo[key] = steps;
        }

        if (steps.Count > 0 && steps[^1].StrokeId == strokeId)
            return steps[^1];

        var step = new UndoStep(strokeId);
        steps.Add(step);
        if (steps.Count > MaxUndoSteps)
            steps.RemoveAt(0);

        return step;
    }

    private void OnUndo(Entity<PaintSurfaceComponent> ent, ref PaintUndoMessage args)
    {
        var user = args.Actor;
        if (ent.Comp.Finished || !CheckRate(user) || !_undo.TryGetValue((ent.Owner, user), out var steps))
            return;

        while (steps.Count > 0)
        {
            var step = steps[^1];
            steps.RemoveAt(steps.Count - 1);
            if (step.Changes.Count == 0)
                continue;

            RestorePixels(ent, step.Changes);
            break;
        }
    }

    private void OnFinish(Entity<PaintSurfaceComponent> ent, ref PaintFinishMessage args)
    {
        var user = args.Actor;
        if (ent.Comp.Finished || ent.Comp.Image is not { } image || !CheckRate(user) || !TryGetActiveTool(user, out _))
            return;

        var title = args.Title.Trim();
        if (title.Length > PaintFinishMessage.MaxTitleLength)
            title = title[..PaintFinishMessage.MaxTitleLength];

        if (title.Length == 0)
            title = Loc.GetString("paint-untitled");

        FlushLog(ent, user);
        SetFinished(ent, title, Identity.Name(user, EntityManager), "paint-finished-name");
        _popup.PopupEntity(Loc.GetString("paint-finished-popup", ("title", title)), user, user);
        _paintLog.LogFinished(ent.Owner, user, title, image);
    }

    public void SetFinished(Entity<PaintSurfaceComponent> ent, string title, string? artist, LocId nameFormat)
    {
        ent.Comp.Finished = true;
        ent.Comp.Title = title;
        ent.Comp.Artist = artist;
        Dirty(ent);

        ClearUndo(ent.Owner);
        _metaData.SetEntityName(ent.Owner, Loc.GetString(nameFormat, ("title", title)));
    }

    public override bool SetImage(Entity<PaintSurfaceComponent> ent, PaintImage image)
    {
        if (!base.SetImage(ent, image))
            return false;

        ClearUndo(ent.Owner);
        return true;
    }

    private void ClearUndo(EntityUid surface)
    {
        var keys = new List<(EntityUid, EntityUid)>();
        foreach (var key in _undo.Keys)
        {
            if (key.Surface == surface)
                keys.Add(key);
        }

        foreach (var key in keys)
        {
            _undo.Remove(key);
        }
    }

    private void UseCharges(Entity<PaintToolComponent> tool, EntityUid user, int pixels)
    {
        if (tool.Comp.PixelsPerCharge <= 0 || !HasComp<LimitedChargesComponent>(tool))
            return;

        tool.Comp.PixelDebt += pixels;
        var cost = tool.Comp.PixelDebt / tool.Comp.PixelsPerCharge;
        tool.Comp.PixelDebt %= tool.Comp.PixelsPerCharge;
        if (cost <= 0)
            return;

        var available = _charges.GetCurrentCharges(tool.Owner);
        _charges.TryUseCharges(tool.Owner, Math.Min(cost, available));

        if (!_charges.IsEmpty(tool.Owner) || !TryComp<CrayonComponent>(tool, out var crayon) || !crayon.DeleteEmpty)
            return;

        _popup.PopupEntity(Loc.GetString("crayon-interact-used-up-text", ("owner", tool.Owner)), user, user);
        QueueDel(tool);
    }

    private void OnUiClosed(EntityUid uid, PaintSurfaceComponent component, BoundUIClosedEvent args)
    {
        if (!Equals(args.UiKey, PaintUiKey.Key))
            return;

        FlushLog((uid, component), args.Actor);
        _undo.Remove((uid, args.Actor));
    }

    private void OnShutdown(Entity<PaintSurfaceComponent> ent, ref ComponentShutdown args)
    {
        var users = new List<EntityUid>();
        foreach (var (surface, user) in _unlogged.Keys)
        {
            if (surface == ent.Owner)
                users.Add(user);
        }

        foreach (var user in users)
        {
            FlushLog(ent, user);
        }

        ClearUndo(ent.Owner);
    }

    private void FlushLog(Entity<PaintSurfaceComponent> ent, EntityUid user)
    {
        if (!_unlogged.Remove((ent.Owner, user), out var pixels) || ent.Comp.Image is not { } image)
            return;

        _paintLog.LogPainting(ent.Owner, user, pixels, image);
    }

    private sealed partial class UndoStep(int strokeId)
    {
        public readonly int StrokeId = strokeId;
        public readonly Dictionary<int, (byte Old, byte New)> Changes = new();
    }
}
