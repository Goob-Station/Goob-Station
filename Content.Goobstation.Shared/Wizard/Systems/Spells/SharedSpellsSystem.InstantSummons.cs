using System.Diagnostics.CodeAnalysis;
using Content.Goobstation.Shared.Wizard.Components;
using Content.Shared.Actions.Components;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Item;
using Content.Shared.Projectiles;
using Robust.Shared.Containers;
using Robust.Shared.Player;

namespace Content.Goobstation.Shared.Wizard.Systems.Spells;

public abstract partial class SharedSpellsSystem
{
    private LocId _locMsgItemMarked = "instant-summons-item-marked";

    private void OnInstantSummons(InstantSummonsEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (!TryComp(ev.Action, out InstantSummonsActionComponent? summons))
            return;

        _hands.TryGetActiveItem(ev.Performer, out var held);

        if (held != null && held == summons.Entity)
            return;

        if (!Exists(summons.Entity) || !TryComp(summons.Entity.Value, out TransformComponent? xform))
        {
            if (ItemValid(held))
                MarkItem(held.Value);
            else
                _popup.PopupClient(Loc.GetString(_locFailNoHeldEntity), ev.Performer);

            return;
        }

        if (ItemValid(held))
        {
            if (TryComp(ev.Action, out ConfirmableActionComponent? confirmable))
            {
                // if not primed, prime it and cancel the action
                if (confirmable.NextConfirm is not { } confirm)
                {
                    _confirmableAction.Prime((ev.Action, confirmable), ev.Performer);
                    return;
                }

                // primed but the delay isnt over, cancel the action
                if (_timing.CurTime < confirm)
                    return;

                // primed and delay has passed, let the action go through
                _confirmableAction.Unprime((ev.Action, confirmable));
            }

            MarkItem(held.Value);
            return;
        }

        ev.Handled = true;

        if (_net.IsClient)
            return;

        var item = summons.Entity.Value;

        if (TryGetOuterNonMobContainer(item, xform, out var container))
            item = container.Owner;

        _audio.PlayEntity(ev.SummonSound, Filter.Pvs(item).Merge(Filter.Pvs(ev.Performer)), item, true);

        if (TryComp(item, out EmbeddableProjectileComponent? embeddable) && embeddable.EmbeddedIntoUid != null)
            _projectile.EmbedDetach(item, embeddable);

        _xform.SetMapCoordinates(item, _xform.GetMapCoordinates(ev.Performer));
        _xform.AttachToGridOrMap(item);

        _hands.TryForcePickupAnyHand(ev.Performer, item);

        return;

        void MarkItem(EntityUid obj)
        {
            summons.Entity = obj;
            _popup.PopupClient(Loc.GetString(_locMsgItemMarked, ("item", obj)), ev.Performer);
            Dirty(ev.Action, summons);
        }

        bool ItemValid([NotNullWhen(true)] EntityUid? obj)
        {
            return HasComp<ItemComponent>(obj) && !HasComp<VirtualItemComponent>(obj);
        }
    }

    // Copied straight from SharedContainerSystem (and modified).
    // TODO: bro......
    private bool TryGetOuterNonMobContainer(EntityUid uid,
        TransformComponent xform,
        [NotNullWhen(true)] out BaseContainer? container)
    {
        container = null;

        if (!uid.IsValid())
            return false;

        var child = uid;
        var parent = xform.ParentUid;

        while (parent.IsValid() && !_bodyQuery.HasComp(parent) && !_bodyPartQuery.HasComp(parent) &&
               !_inventoryQuery.HasComp(parent) && !_handsQuery.HasComp(parent) && !_binglePitQuery.HasComp(parent))
        {
            if (((EntityManager.MetaQuery.GetComponent(child).Flags & MetaDataFlags.InContainer) ==
                 MetaDataFlags.InContainer) && _containerManagerQuery.TryGetComponent(parent, out var conManager) &&
                _container.TryGetContainingContainer(parent, child, out var parentContainer, conManager))
            {
                container = parentContainer;
            }

            var parentXform = _xformQuery.GetComponent(parent);
            child = parent;
            parent = parentXform.ParentUid;
        }

        return container != null;
    }
}