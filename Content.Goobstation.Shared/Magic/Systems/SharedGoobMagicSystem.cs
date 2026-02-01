using Content.Shared.Actions.Components;
using Content.Shared.Actions.Events;
using Content.Shared.Chat;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Content.Shared.Speech.Muting;
using System.Diagnostics.CodeAnalysis;

namespace Content.Goobstation.Shared.Magic.Systems;

public sealed partial class SharedGoobMagicSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedChatSystem _chat = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MagicActionComponent, ActionUpdateEvent>(OnActionUpdate);
        SubscribeLocalEvent<MagicActionComponent, ActionAttemptEvent>(OnActionAttempt);
        SubscribeLocalEvent<MagicActionComponent, ActionPerformedEvent>(OnActionPerformed);
    }

    private void OnActionUpdate(Entity<MagicActionComponent> ent, ref ActionUpdateEvent args)
    {
        if (!TryComp<ActionComponent>(args.Action, out var ac) || !ac.Container.HasValue)
            return;

        args.QueueDisable = !CanInvoke(ent, ac.Container.Value, out _);
    }

    private void OnActionAttempt(Entity<MagicActionComponent> ent, ref ActionAttemptEvent args)
    {
        if (!CanInvoke(ent, args.User, out var reason))
        {
            _popup.PopupEntity(reason, args.User, args.User, PopupType.SmallCaution);
            args.Cancelled = true;
            return;
        }
    }

    private void OnActionPerformed(Entity<MagicActionComponent> ent, ref ActionPerformedEvent args)
    {
        if (ent.Comp.InvocationLoc.HasValue)
            _chat.TrySendInGameICMessage(args.Performer, ent.Comp.InvocationLoc.Value, ent.Comp.InvocationType, false);
    }

    public bool CanInvoke(Entity<MagicActionComponent> ent, EntityUid performer, [NotNullWhen(false)] out string? reason)
    {
        reason = null;
        var muted = ent.Comp.InvocationLoc.HasValue && HasComp<MutedComponent>(performer);
        if (muted)
        {
            reason = Loc.GetString("magic-requirements-muted");
            return false;
        }

        if (ent.Comp.RequiredMagicalItemWeight > 0)
        {
            var items = 0;

            // in case of an ascended heretic or such.
            // since they no longer need a focus, why not turn them into one? :D
            if (TryComp<MagicalItemComponent>(performer, out var pmic))
                items += pmic.Weight;

            var ise = _inventory.GetSlotEnumerator(performer, SlotFlags.WITHOUT_POCKET);
            while (ise.MoveNext(out var container))
            {
                var item = container.ContainedEntity;
                if (!item.HasValue || !TryComp<MagicalItemComponent>(item.Value, out var mic)) continue;
                items += mic.Weight;
            }

            var hands = _hands.EnumerateHeld(performer);
            foreach (var held in hands)
                items += TryComp<MagicalItemComponent>(held, out var mic) ? mic.Weight : 0;

            if (items <= ent.Comp.RequiredMagicalItemWeight)
            {
                reason = Loc.GetString("magic-requirements-items", ("n", ent.Comp.RequiredMagicalItemWeight - items));
                return false;
            }
        }

        return true;
    }
}
