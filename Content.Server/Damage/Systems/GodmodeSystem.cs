// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2023 Dawid Bla <46636558+DawBla@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2021 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Silver <silvertorch5@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Atmos.Components;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;

namespace Content.Server.Damage.Systems;

public sealed class GodmodeSystem : SharedGodmodeSystem
{
    public override void EnableGodmode(EntityUid uid, GodmodeComponent? godmode = null)
    {
        godmode ??= EnsureComp<GodmodeComponent>(uid);

        base.EnableGodmode(uid, godmode);

        if (TryComp<MovedByPressureComponent>(uid, out var moved))
        {
            godmode.WasMovedByPressure = moved.Enabled;
            moved.Enabled = false;
        }
    }

    public override void DisableGodmode(EntityUid uid, GodmodeComponent? godmode = null)
    {
    	if (!Resolve(uid, ref godmode, false))
    	    return;

        base.DisableGodmode(uid, godmode);

        if (godmode.Deleted)
            return;

        if (TryComp<MovedByPressureComponent>(uid, out var moved))
        {
            moved.Enabled = godmode.WasMovedByPressure;
        }
    }
}