// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Light;
using Content.Shared.Light.Components;

namespace Content.Server.Light.EntitySystems;

public sealed class RotatingLightSystem : SharedRotatingLightSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RotatingLightComponent, PointLightToggleEvent>(OnLightToggle);
    }

    private void OnLightToggle(EntityUid uid, RotatingLightComponent comp, PointLightToggleEvent args)
    {
        if (comp.Enabled == args.Enabled)
            return;

        comp.Enabled = args.Enabled;
        Dirty(uid, comp);
    }
}