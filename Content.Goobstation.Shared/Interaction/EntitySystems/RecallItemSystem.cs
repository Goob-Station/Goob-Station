using Content.Shared.Actions;
using Content.Shared.Hands.EntitySystems;
using Content.Goobstation.Shared.Interaction;
using Content.Goobstation.Shared.Interaction.Components;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;

namespace Content.Goobtstation.Shared.Interaction.EntitySystems;

public sealed class RecallItemSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<RecallBoundItemComponent, RecallBoundItemEvent>(OnRecall);
    }

    private void OnRecall(Entity<RecallBoundItemComponent> ent, ref RecallBoundItemEvent args)
    {
        var user = args.Performer;
        var action = args.Action.Owner;

        foreach (var (item, actionEnt) in ent.Comp.BoundItems)
        {
            if (actionEnt != action)
                continue;

            if (_hands.IsHolding(user, item))
            {
                _popup.PopupEntity(Loc.GetString("recall-item-already-held"), user, user);
                args.Handled = true;
                return;
            }

            if (_hands.TryPickupAnyHand(user, item))
                _popup.PopupEntity(Loc.GetString("recall-item-success"), user, user);
            else
                _popup.PopupEntity(Loc.GetString("recall-item-hands-full"), user, user);

            args.Handled = true;
            return;
        }
    }
}
