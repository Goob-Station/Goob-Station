using Content.Shared.Hands;
using Content.Shared.Popups;
using Content.Shared.Throwing;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Interaction;

/// <summary>
/// This handles items that cant be dropped
/// </summary>
public sealed class UndropableSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<UndropableComponent, DropItemAttemptEvent>(OnDrop);
        SubscribeLocalEvent<UndropableComponent, ThrowItemAttemptEvent>(OnTrow);
        SubscribeLocalEvent<UndropableComponent, ContainerGettingRemovedAttemptEvent>(OnInsert);

    }

    private void OnDrop(Entity<UndropableComponent> ent, ref  DropItemAttemptEvent arg)
    {
        if (!ent.Comp.Enabled)
            return;

        arg.Cancelled = true;

        Popup(ent, arg.User);
    }

    private void OnTrow(Entity<UndropableComponent> ent, ref ThrowItemAttemptEvent arg)
    {
        if (!ent.Comp.Enabled)
            return;

        arg.Cancelled = ent.Comp.Enabled;

        Popup(ent, arg.User);
    }

    private void OnInsert(Entity<UndropableComponent> ent, ref ContainerGettingRemovedAttemptEvent arg)
    {
        if (!ent.Comp.Enabled)
            return;

        arg.Cancel();
    }

    private void Popup(Entity<UndropableComponent> ent, EntityUid user)
    {
        if (_timing.CurTime < ent.Comp.LastPopup + ent.Comp.PopupCooldown)
            return;

        _popup.PopupPredicted(Loc.GetString("interaction-misc-drop-prevented"), user,user);
        ent.Comp.LastPopup = _timing.CurTime;
    }
}
