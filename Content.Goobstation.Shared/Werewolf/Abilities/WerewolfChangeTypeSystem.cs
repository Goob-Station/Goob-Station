using Content.Goobstation.Shared.Werewolf.Abilities.Basic;
using Content.Shared.Actions;
using Content.Shared.Popups;

namespace Content.Goobstation.Shared.Werewolf.Abilities;

// handles the werewolves changing types
public sealed class WerewolfChangeTypeSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<WerewolfBasicAbilitiesComponent, EventWerewolfChangeType>(OnTypeChanged);
    }

    private void OnTypeChanged(Entity<WerewolfBasicAbilitiesComponent> ent, ref EventWerewolfChangeType args)
    {
        if (!TryComp<WerewolfBasicAbilitiesComponent>(ent.Owner, out var comp))
            return;
        if (comp.Transfurmed == true)
        {
            _popup.PopupClient(Loc.GetString("werewolf-action-fail-transfurmed"), ent.Owner);
            return;
        }
        comp.CurrentMutation = args.WerewolfType;
        _popup.PopupClient(Loc.GetString("werewolf-action-change-type"), ent.Owner, PopupType.LargeCaution);
        args.Handled = true;
    }
}
