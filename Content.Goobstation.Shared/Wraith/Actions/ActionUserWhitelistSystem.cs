using Content.Shared.Actions.Events;
using Content.Shared.Popups;
using Content.Shared.Whitelist;

namespace Content.Goobstation.Shared.Wraith.Actions;

public sealed class ActionUserWhitelistSystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActionUserWhitelistComponent, ActionAttemptEvent>(OnActionAttempt);
    }

    private void OnActionAttempt(Entity<ActionUserWhitelistComponent> ent, ref ActionAttemptEvent args)
    {
        if (_whitelist.IsWhitelistPass(ent.Comp.Whitelist, args.User))
            return;

        if (ent.Comp.Popup.HasValue)
            _popup.PopupClient(Loc.GetString(ent.Comp.Popup), args.User, PopupType.MediumCaution);

        args.Cancelled = true;
    }
}
