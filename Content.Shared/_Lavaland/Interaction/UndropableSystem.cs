using Content.Shared.Hands;
using Content.Shared.Popups;
using Content.Shared.Throwing;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Shared._Lavaland.Interaction;

/// <summary>
/// This handles items that cant be dropped
/// </summary>
public sealed class UndroppableSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<UndroppableComponent, DropItemAttemptEvent>(OnDrop);
        SubscribeLocalEvent<UndroppableComponent, ThrowItemAttemptEvent>(OnThrow);
        SubscribeLocalEvent<UndroppableComponent, ContainerGettingRemovedAttemptEvent>(OnInsert);
    }

    private void OnDrop(Entity<UndroppableComponent> ent, ref  DropItemAttemptEvent arg)
    {
        if (!ent.Comp.Enabled)
            return;

        arg.Cancelled = true;

        Popup(ent, arg.User);
    }

    private void OnThrow(Entity<UndroppableComponent> ent, ref ThrowItemAttemptEvent arg)
    {
        if (!ent.Comp.Enabled)
            return;

        arg.Cancelled = ent.Comp.Enabled;

        Popup(ent, arg.User);
    }

    private void OnInsert(Entity<UndroppableComponent> ent, ref ContainerGettingRemovedAttemptEvent arg)
    {
        if (!ent.Comp.Enabled)
            return;

        arg.Cancel();
    }

    private void Popup(Entity<UndroppableComponent> ent, EntityUid user)
    {
        if (_timing.CurTime < ent.Comp.LastPopup + ent.Comp.PopupCooldown)
            return;

        _popup.PopupPredicted(Loc.GetString("interaction-misc-drop-prevented"), user,user);
        ent.Comp.LastPopup = _timing.CurTime;
    }
}
