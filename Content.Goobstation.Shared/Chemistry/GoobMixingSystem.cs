using Content.Shared.Chemistry.Reaction;
using Content.Shared.Popups;
using Content.Shared.Whitelist;

namespace Content.Goobstation.Shared.Chemistry;

public sealed partial class GoobMixingSystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ReactionMixerComponent, MixingAttemptEvent>(OnMixingAttempt);
    }

    private void OnMixingAttempt(Entity<ReactionMixerComponent> ent, ref MixingAttemptEvent args)
    {
        if (_whitelist.IsWhitelistFail(ent.Comp.Whitelist, ent.Comp.User))
        {
            _popup.PopupClient(Loc.GetString("reaction-mixer-fail-non-user", ("mixer", ent.Owner)), ent.Comp.User);
            args.Cancelled = true;
        }
    }
}
