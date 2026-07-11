// SPDX-FileCopyrightText: 2024 White Dream Project contributors
// SPDX-FileCopyrightText: 2026 v0id <>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.BloodCult.Components;
using Content.Shared.Doors;
using Content.Shared.Whitelist;

namespace Content.Shared.BloodCult;

public sealed class SharedBloodCultDoorSystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodCultDoorComponent, BeforeDoorOpenedEvent>(OnBeforeDoorOpened);
    }

    private void OnBeforeDoorOpened(EntityUid uid, BloodCultDoorComponent component, BeforeDoorOpenedEvent args)
    {
        if (args.User is { } user && !_whitelist.IsWhitelistPass(component.Whitelist, user))
            args.Cancel();
    }
}
