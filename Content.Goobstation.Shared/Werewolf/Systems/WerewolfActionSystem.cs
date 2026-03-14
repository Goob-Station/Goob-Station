using Content.Goobstation.Shared.Werewolf.Components;
using Content.Goobstation.Shared.Werewolf.Abilities.Basic;
using Content.Shared.Actions.Events;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Popups;
using Robust.Shared.Containers;

namespace Content.Goobstation.Shared.Werewolf.Systems;

public sealed class SharedWerewolfActionSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly HungerSystem _hunger = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WerewolfActionComponent, ActionAttemptEvent>(OnActionAttempt);
    }

    private void OnActionAttempt(Entity<WerewolfActionComponent> ent, ref ActionAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        var user = args.User;
        var comp = ent.Comp;

        if (comp.RequireTransfurmed)
        {
            if (!TryComp<WerewolfBasicAbilitiesComponent>(user, out var wolf) || !wolf.Transfurmed)
            {
                _popup.PopupClient(Loc.GetString(comp.NotTransfurmedPopup), user, user);
                args.Cancelled = true;
                return;
            }
        }

        if (comp.HungerCost > 0)
        {
            if (!TryComp<HungerComponent>(user, out var hunger))
                return;

            if (_hunger.GetHunger(hunger) < comp.HungerCost)
            {
                _popup.PopupClient(Loc.GetString(comp.NoHungerPopup), user, user);
                args.Cancelled = true;
                return;
            }
        }
        _hunger.ModifyHunger(user, -comp.HungerCost);
    }
}
