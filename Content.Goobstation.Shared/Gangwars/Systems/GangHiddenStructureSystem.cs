using Content.Goobstation.Shared.Gangwars.Components;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Gangwars.Systems;

/// <summary>
/// Stealths the structure without overriding the color and allows gang members to reveal it.
/// </summary>
public sealed class GangHiddenStructureSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    private static readonly ProtoId<TagPrototype> HideContextMenuTag = "HideContextMenu";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GangHiddenStructureComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<GangHiddenStructureComponent, InteractHandEvent>(OnInteractHand);
        SubscribeLocalEvent<GangHiddenStructureComponent, ExamineAttemptEvent>(OnExamineAttempt);
    }

    private void OnExamineAttempt(Entity<GangHiddenStructureComponent> ent, ref ExamineAttemptEvent args)
    {
        if (ent.Comp.IsHidden)
            args.Cancel();
    }

    private void OnMapInit(Entity<GangHiddenStructureComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.LastInteractTime = _timing.CurTime;
        Dirty(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_netManager.IsServer)
            return;

        var query = EntityQueryEnumerator<GangHiddenStructureComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.IsHidden)
                continue;

            if (_timing.CurTime < comp.LastInteractTime + TimeSpan.FromSeconds(comp.HideDelay))
                continue;

            Hide((uid, comp));
        }
    }

    /// <summary>
    /// Resets the inactivity timer so the structure stays revealed.
    /// </summary>
    public void KeepRevealed(Entity<GangHiddenStructureComponent?> ent)
    {
        if (!Resolve(ent.Owner, ref ent.Comp, false))
            return;

        ent.Comp.LastInteractTime = _timing.CurTime;
        Dirty(ent.Owner, ent.Comp);
    }

    public void Hide(Entity<GangHiddenStructureComponent> ent)
    {
        EnsureComp<StealthPreserveColorComponent>(ent);
        _tag.AddTag(ent.Owner, HideContextMenuTag);

        ent.Comp.IsHidden = true;
        Dirty(ent);
    }

    public void Reveal(Entity<GangHiddenStructureComponent> ent)
    {
        RemCompDeferred<StealthPreserveColorComponent>(ent);
        _tag.RemoveTag(ent.Owner, HideContextMenuTag);

        ent.Comp.IsHidden = false;
        ent.Comp.LastInteractTime = _timing.CurTime;
        Dirty(ent);
    }

    private void OnInteractHand(Entity<GangHiddenStructureComponent> ent, ref InteractHandEvent args)
    {
        if (args.Handled || !ent.Comp.IsHidden)
            return;

        if (!TryComp<GangMemberComponent>(args.User, out var member)
            || !TryComp<GangColorComponent>(ent.Owner, out var gangColor)
            || member.Gang != gangColor.GangColor)
            return;

        Reveal(ent);
        _popup.PopupClient(Loc.GetString(ent.Comp.RevealPopup), args.User, args.User);
        args.Handled = true;
    }
}
