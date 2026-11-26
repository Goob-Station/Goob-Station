using Content.Goobstation.Shared.Paint;
using Content.Server.Power.EntitySystems;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Power;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Paint;

public sealed partial class PhotocopierSystem : EntitySystem
{
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private ItemSlotsSystem _itemSlots = default!;
    [Dependency] private PaintLogSystem _paintLog = default!;
    [Dependency] private PaintSurfaceSystem _paint = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private PowerReceiverSystem _power = default!;
    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PhotocopierComponent, ActivateInWorldEvent>(OnActivate);
        SubscribeLocalEvent<PhotocopierComponent, GetVerbsEvent<Verb>>(OnGetVerbs);
        SubscribeLocalEvent<PhotocopierComponent, PowerChangedEvent>(OnPowerChanged);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<PhotocopierComponent>();
        while (query.MoveNext(out var uid, out var copier))
        {
            if (copier.State == PhotocopierState.Idle || now < copier.NextStage)
                continue;

            if (copier.State == PhotocopierState.Scanning)
            {
                copier.State = PhotocopierState.Printing;
                copier.NextStage = now + copier.PrintTime;
                _appearance.SetData(uid, PhotocopierVisuals.State, copier.State);
                _audio.PlayPvs(copier.PrintSound, uid);
                continue;
            }

            Finish((uid, copier));
        }
    }

    private void OnActivate(Entity<PhotocopierComponent> ent, ref ActivateInWorldEvent args)
    {
        if (args.Handled || !args.Complex)
            return;

        args.Handled = true;
        TryStart(ent, args.User);
    }

    private void OnGetVerbs(Entity<PhotocopierComponent> ent, ref GetVerbsEvent<Verb> args)
    {
        if (!args.CanAccess || !args.CanInteract || ent.Comp.State != PhotocopierState.Idle)
            return;

        var user = args.User;
        args.Verbs.Add(new Verb
        {
            Text = Loc.GetString("photocopier-verb-copy"),
            Act = () => TryStart(ent, user),
        });
    }

    private void OnPowerChanged(Entity<PhotocopierComponent> ent, ref PowerChangedEvent args)
    {
        if (!args.Powered && ent.Comp.State != PhotocopierState.Idle)
            Reset(ent);
    }

    public bool TryStart(Entity<PhotocopierComponent> ent, EntityUid user)
    {
        if (ent.Comp.State != PhotocopierState.Idle)
        {
            _popup.PopupEntity(Loc.GetString("photocopier-busy"), ent, user);
            return false;
        }

        if (!_power.IsPowered(ent))
        {
            _popup.PopupEntity(Loc.GetString("photocopier-unpowered"), ent, user);
            return false;
        }

        var first = _itemSlots.GetItemOrNull(ent, ent.Comp.FirstSlot);
        var second = _itemSlots.GetItemOrNull(ent, ent.Comp.SecondSlot);
        if (first is not { } a || second is not { } b)
        {
            _popup.PopupEntity(Loc.GetString("photocopier-needs-canvases"), ent, user);
            return false;
        }

        var aBlank = IsBlank(a);
        var bBlank = IsBlank(b);
        if (aBlank == bBlank)
        {
            _popup.PopupEntity(Loc.GetString(aBlank ? "photocopier-nothing-to-copy" : "photocopier-needs-blank"), ent, user);
            return false;
        }

        var original = aBlank ? b : a;
        var target = aBlank ? a : b;
        if (!TryComp<PaintSurfaceComponent>(original, out var originalSurface)
            || !TryComp<PaintSurfaceComponent>(target, out var targetSurface)
            || originalSurface.Size != targetSurface.Size)
        {
            _popup.PopupEntity(Loc.GetString("photocopier-size-mismatch"), ent, user);
            return false;
        }

        ent.Comp.Original = original;
        ent.Comp.Target = target;
        ent.Comp.User = user;
        ent.Comp.State = PhotocopierState.Scanning;
        ent.Comp.NextStage = _timing.CurTime + ent.Comp.ScanTime;
        SetLocked(ent, true);
        _appearance.SetData(ent, PhotocopierVisuals.State, ent.Comp.State);
        _audio.PlayPvs(ent.Comp.ScanSound, ent);
        return true;
    }

    private void Finish(Entity<PhotocopierComponent> ent)
    {
        var original = ent.Comp.Original;
        var target = ent.Comp.Target;
        var user = ent.Comp.User;
        Reset(ent);

        if (original is not { } originalUid
            || target is not { } targetUid
            || !TryComp<PaintSurfaceComponent>(originalUid, out var originalSurface)
            || !TryComp<PaintSurfaceComponent>(targetUid, out var targetSurface)
            || originalSurface.Image is not { } image
            || !IsBlank(targetUid)
            || !_paint.SetImage((targetUid, targetSurface), image.Clone()))
            return;

        var title = originalSurface.Title ?? Loc.GetString("paint-untitled");
        var artist = originalSurface.Artist is { } name
            ? Loc.GetString("paint-copy-artist", ("artist", name))
            : null;

        _paint.SetFinished((targetUid, targetSurface), title, artist, "paint-copy-name");
        _paintLog.LogCopied(originalUid, targetUid, user, image);
    }

    private void Reset(Entity<PhotocopierComponent> ent)
    {
        ent.Comp.State = PhotocopierState.Idle;
        ent.Comp.Original = null;
        ent.Comp.Target = null;
        ent.Comp.User = null;
        SetLocked(ent, false);
        _appearance.SetData(ent, PhotocopierVisuals.State, ent.Comp.State);
    }

    private void SetLocked(Entity<PhotocopierComponent> ent, bool locked)
    {
        _itemSlots.SetLock(ent, ent.Comp.FirstSlot, locked);
        _itemSlots.SetLock(ent, ent.Comp.SecondSlot, locked);
    }

    private bool IsBlank(EntityUid uid)
    {
        if (!TryComp<PaintSurfaceComponent>(uid, out var surface) || surface.Finished || surface.Image is not { } image)
            return false;

        foreach (var pixel in image.Pixels)
        {
            if (pixel != 0)
                return false;
        }

        return true;
    }
}
