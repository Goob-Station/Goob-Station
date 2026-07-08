// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Mind;
using Content.Shared.UserInterface;
using Content.Shared.Whitelist;

namespace Content.Shared._Goobstation.Wizard.UserInterface;

public sealed class ActivatableUiUserWhitelistSystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActivatableUiUserWhitelistComponent, ActivatableUIOpenAttemptEvent>(OnAttempt);
    }

    private void OnAttempt(Entity<ActivatableUiUserWhitelistComponent> ent, ref ActivatableUIOpenAttemptEvent args)
    {
        if (!CheckWhitelist(ent.Owner, args.User, ent.Comp))
            args.Cancel();
    }

    public bool CheckWhitelist(EntityUid uid, EntityUid user, ActivatableUiUserWhitelistComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return true;

        var result = _whitelist.IsValid(component.Whitelist, user);

        if (result)
            return true;

        if (!component.CheckMind)
            return false;

        return _mind.TryGetMind(user, out var mind, out _) && _whitelist.IsValid(component.Whitelist, mind);
    }
}
