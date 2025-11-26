// SPDX-FileCopyrightText: 2025 RichardBlonski <48651647+RichardBlonski@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Religion;
using Content.Goobstation.Server.Chaplain.Components;

namespace Content.Goobstation.Server.Chaplain.Events;

public sealed class RoleEventHandler : EntitySystem
{
    [Dependency] private readonly SharedEyeSystem _eye = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BibleUserComponent, ComponentStartup>(OnBibleUserAdded);
    }

    private void OnBibleUserAdded(EntityUid uid, BibleUserComponent component, ComponentStartup args)
    {
        // Add the component to anyone with the BibleUser component
        // Pretty much anything that is a Chaplain should have this component (ERT etc)
        var see = EnsureComp<SeeHereticFixturesComponent>(uid);
        see.SeeShifts = false;
        _eye.RefreshVisibilityMask(uid);
    }
}
