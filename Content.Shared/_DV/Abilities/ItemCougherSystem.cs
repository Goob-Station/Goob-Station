// SPDX-FileCopyrightText: 2025 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._DV.Actions.Events;
using Content.Shared.Actions;
using Content.Shared.Clothing.Components;
using Content.Shared.IdentityManagement;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared._DV.Abilities;

public sealed class ItemCougherSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    private EntityQuery<ItemCougherComponent> _query;

    public override void Initialize()
    {
        base.Initialize();

        _query = GetEntityQuery<ItemCougherComponent>();

        SubscribeLocalEvent<ItemCougherComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ItemCougherComponent, CoughItemActionEvent>(OnCoughItemAction);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CoughingUpItemComponent, ItemCougherComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var coughing, out var comp, out var xform))
        {
            if (_timing.CurTime < coughing.NextCough)
                continue;

            var spawned = PredictedSpawnAtPosition(comp.Item, xform.Coordinates);
            RemCompDeferred(uid, coughing);

            var ev = new ItemCoughedUpEvent(spawned);
            RaiseLocalEvent(uid, ref ev);
        }
    }

    private void OnMapInit(Entity<ItemCougherComponent> ent, ref MapInitEvent args)
    {
        if (ent.Comp.ActionEntity != null)
            return;

        _actions.AddAction(ent, ref ent.Comp.ActionEntity, ent.Comp.Action);
    }

    private void OnCoughItemAction(Entity<ItemCougherComponent> ent, ref CoughItemActionEvent args)
    {
        if (_inventory.TryGetSlotEntity(ent, "mask", out var maskUid)
            && TryComp<MaskComponent>(maskUid, out var mask)
            && !mask.IsToggled)
        {
            _popup.PopupClient(Loc.GetString("item-cougher-mask", ("mask", maskUid)), ent, ent);
            return;
        }

        var msg = Loc.GetString(ent.Comp.CoughPopup, ("name", Identity.Entity(ent, EntityManager)));
        _popup.PopupPredicted(msg, ent, ent);
        _audio.PlayPredicted(ent.Comp.Sound, ent, ent);

        var path = _audio.GetSound(ent.Comp.Sound);
        var coughing = EnsureComp<CoughingUpItemComponent>(ent);
        coughing.NextCough = _timing.CurTime + _audio.GetAudioLength(path);
        args.Handled = true;

        // disable it until another system calls EnableAction
        SetActionEnabled((ent, ent.Comp), false);
    }

    /// <summary>
    /// Enables the coughing action.
    /// Other systems have to call this, this is not used internally.
    /// </summary>
    public void EnableAction(Entity<ItemCougherComponent?> ent)
        => SetActionEnabled(ent, true);

    public void SetActionEnabled(Entity<ItemCougherComponent?> ent, bool enabled)
    {
        if (!_query.Resolve(ent, ref ent.Comp)
        || ent.Comp.ActionEntity is not {} action)
            return;

        _actions.SetEnabled(action, enabled);
    }
}

/// <summary>
/// Raised on the mob after it coughs up an item.
/// </summary>
[ByRefEvent]
public record struct ItemCoughedUpEvent(EntityUid Item);
